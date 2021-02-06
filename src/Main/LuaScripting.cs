﻿using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;
using XMLTypes;

namespace LuaScripting
{
    public class LuaScripting : SingleInstance<LuaScripting>
    {
        public static bool SHOW_GUI;
        public override string Name => "LuaScripting";

        private ILuaState _lua;
        private ThreadStatus _status;

        private Rect windowRect = new Rect(6, 70, 200, 190);

        private int startRef, updateRef, lateUpdate, fixedUpdateRef, onguiRef;
        private bool scriptOk;
        private static bool markForLoadFromMachine;

        public static AudioSource audioSource;

        public static List<AwaitAction> awaitActions = new List<AwaitAction>();
        public static Dictionary<string, List<Block>> localMachineBlockRefs = new Dictionary<string, List<Block>>();
        public static Dictionary<KeyCode, List<Block>> localMachineUsedKeys = new Dictionary<KeyCode, List<Block>>();
        public static Camera hudCamera;
        public static Camera mainCamera;

        private static int hasErrors;

        public static GUIStyle hintStyle = new GUIStyle()
        {
            normal = {
                    background = ModResource.GetTexture("background-darker"),
                    textColor = Color.white
            },
            font = Font.CreateDynamicFontFromOSFont("Lucida Console", 14),
            richText = true,
            alignment = TextAnchor.MiddleCenter,
        };

        public static GUIStyle errStyle = new GUIStyle()
        {
            normal = {
                    background = ModResource.GetTexture("background"),
                    textColor = Color.red
            },
            font = Font.CreateDynamicFontFromOSFont("Lucida Console", 25),
            richText = true,
            alignment = TextAnchor.MiddleCenter,
        };

        public static void AddAwaitAction(Action action, int time)
        {
            awaitActions.Add(new AwaitAction()
            {
                action = action,
                fixedUpdateTicksRemain = time
            });
        }

        public static void Init()
        {
            Application.logMessageReceived += (string condition, string stackTrace, LogType type) =>
            {
                if (stackTrace.Contains("LuaScripting"))
                    if (type == LogType.Error)
                    {
                        audioSource.volume = 0.5F;
                        if (!audioSource.isPlaying)
                            audioSource.PlayOneShot(ModResource.GetAudioClip("error"));
                        hasErrors = 300;
                    }
            };

            Events.OnMachineSave += (info) =>
            {
                Mod.SaveLuaRootToMachine();
            };

            Events.OnMachineSimulationToggle += (pl, state) =>
            {
                if (pl.Player == null || (pl.Player != null && pl.Player.IsLocalPlayer))
                {
                    if (!state)
                    {
                        awaitActions.Clear();
                        localMachineBlockRefs.Clear();
                        localMachineUsedKeys.Clear();

                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                }
            };

            Events.OnMachineDestroyed += () =>
            {
                Machine.Active().MachineData.Clear();
            };

            Events.OnBlockInit += (block) =>
            {
                OnBlockInit(block);
            };
        }

        private static void OnBlockInit(Block block)
        {
            if (block.InternalObject.BuildIndex == 0)
            {
                if (block.InternalObject.ParentMachine == Machine.Active())
                {
                    if (!Machine.Active().SimulationMachine)
                    {
                        markForLoadFromMachine = true;
                    }
                    else
                    {
                        Machine.Active().SimulationMachine.gameObject.AddComponent<Libs.LuaMachineInfoCollector>();

                        foreach (Block b in block.Machine.BuildingBlocks)
                        {
                            MText refKey = b.InternalObject.GetMapperType("bmt-lua_ref_key") as MText;

                            if (refKey != null)
                            {
                                if (!localMachineBlockRefs.ContainsKey(refKey.Value))
                                    localMachineBlockRefs.Add(refKey.Value, new List<Block>()); ;
                                localMachineBlockRefs[refKey.Value].Add(b);
                            }

                            foreach (MapperType mt in b.InternalObject.MapperTypes)
                                if (mt is MKey)
                                {
                                    MKey mkey = mt as MKey;
                                    for (int i = 0; i < mkey.KeysCount; i++)
                                    {
                                        KeyCode keyCode = mkey.GetKey(i);
                                        if (!localMachineUsedKeys.ContainsKey(keyCode))
                                            localMachineUsedKeys.Add(keyCode, new List<Block>());
                                        localMachineUsedKeys[keyCode].Add(b);
                                    }
                                }
                        }

                        Instance.InitializeLua();
                    }
                }
            } else
            {
                string coolBlockRefKey = (block.Prefab.Name + "_" + block.Guid.ToString().Substring(0, 8)).Replace(' ', '_');
                block.InternalObject.AddText(new MText("Ref. Key", "lua_ref_key", coolBlockRefKey));
                block.InternalObject.Prefab.EmulatesAnyKeys = true;
            }
        }

        public void InitializeLua()
        {
            scriptOk = true;

            _lua = LuaAPI.NewState();
            _lua.L_OpenLibs();
            _lua.L_RequireF("gui", Libs.GUILib.OpenLib, true);
            _lua.L_RequireF("rect", Libs.RectLib.OpenLib, true);
            _lua.L_RequireF("vector", Libs.VectorLib.OpenLib, true);
            _lua.L_RequireF("machine", Libs.MachineLib.OpenLib, true);
            _lua.L_RequireF("input", Libs.InputLib.OpenLib, true);
            _lua.L_RequireF("cursor", Libs.CursorLib.OpenLib, true);
            _lua.L_RequireF("physics", Libs.PhysicsLib.OpenLib, true);
            _lua.L_RequireF("lines", Libs.LinesLib.OpenLib, true);
            _lua.L_RequireF("players", Libs.PlayersLib.OpenLib, true);
            _lua.L_RequireF("screen", Libs.ScreenLib.OpenLib, true);

            _status = _lua.L_DoFile("main.lua");

            if (_status != ThreadStatus.LUA_OK)
            {
                Debug.LogError(_lua.ToString(-1));
                scriptOk = false;
            }

            if (!_lua.IsTable(-1))
            {
                Debug.LogError("[LuaScripting] Framework main's return value is not a table.");
                scriptOk = false;
            }

            if (scriptOk)
            {
                startRef = StoreLuaMethod("play");
                updateRef = StoreLuaMethod("update");
                lateUpdate = StoreLuaMethod("late_update");
                fixedUpdateRef = StoreLuaMethod("fixed_update");
                onguiRef = StoreLuaMethod("on_gui");

                Mod.SaveLuaRootToMachine();
            }

            _lua.Pop(1);

            if (scriptOk && (Machine.Active().SimulationMachine != null))
                CallLuaMethod(startRef);
        }

        private void OnGUI()
        {
            if (!Machine.Active())
                return;

            if (SHOW_GUI)
            {
                windowRect = GUI.Window(84475229, windowRect, DoWindow, "Lua Scripting", Libs.GUILib.windowStyle);

                if (BlockMapper.CurrentInstance && BlockMapper.CurrentInstance.IsBlock && BlockMapper.CurrentInstance.Block.MapperTypes.Count > 0)
                {
                    Rect inspectorRect = windowRect;
                    inspectorRect.width = 300;
                    inspectorRect.x = windowRect.x + windowRect.width + 10;
                    inspectorRect.height = 60 + BlockMapper.CurrentInstance.Block.MapperTypes.Count * 30;
                    GUI.Window(84475239, inspectorRect, InspectKeysWindow, "Mapper Keys", Libs.GUILib.windowStyle);
                }
            }

            if (hasErrors > 0)
            {
                GUI.Label(new Rect(Screen.width / 2 - 240 * 3 / 2, Screen.height / 2 - 30 * 3 / 2, 240 * 3, 30 * 3), "An error occured. Check console (Ctrl + K).", errStyle);
            }

            if (scriptOk && (Machine.Active().SimulationMachine != null))
                CallLuaMethod(onguiRef);
        }

        private void InspectKeysWindow(int uselessId)
        {
            int i = 1;
            GUI.Label(new Rect(10, 30, 135, 20), "Name", Libs.GUILib.labelStyle);
            GUI.Label(new Rect(155, 30, 135, 20), "Key", Libs.GUILib.labelStyle);
            foreach (MapperType mt in BlockMapper.CurrentInstance.Block.MapperTypes)
            {
                GUI.Label(new Rect(10, 30 + 30 * i, 135, 20), $"{mt.DisplayName}", Libs.GUILib.labelStyle);
                GUI.Label(new Rect(155, 30 + 30 * i, 135, 20), $"{mt.Key}", Libs.GUILib.labelStyle);
                i++;
            }
        }

        private void DoWindow(int uselessId)
        {
            if (GUI.Button(new Rect(10, 30, 180, 30), "Open LuaRoot Folder", Libs.GUILib.buttonStyle))
            {
                if (!ModIO.ExistsDirectory("LuaRoot"))
                    ModIO.CreateDirectory("LuaRoot");
                ModIO.OpenFolderInFileBrowser("LuaRoot");
            }

            if (GUI.Button(new Rect(10, 70, 180, 30), "Save LuaRoot Manually", Libs.GUILib.buttonStyle))
            {
                Mod.SaveLuaRootToMachine();
            }

            if (GUI.Button(new Rect(10, 110, 180, 30), "Load LuaRoot Manually", Libs.GUILib.buttonStyle))
            {
                Mod.LoadLuaRootFromMachine();
            }

            if (GUI.Button(new Rect(10, 150, 180, 30), "Documentation", Libs.GUILib.buttonStyle))
            {
                ModIO.OpenFolderInFileBrowser("Doc");
            }

            GUI.DragWindow();
        }

        private void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            mainCamera = Camera.main;
            hudCamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();
            
        }

        private void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.L))
                SHOW_GUI = !SHOW_GUI;

            if (!Machine.Active())
                return;

            if (scriptOk && (Machine.Active().SimulationMachine != null))
                CallLuaMethod(updateRef);
        }

        private void LateUpdate()
        {
            if (!Machine.Active())
                return;

            if (scriptOk && (Machine.Active().SimulationMachine != null))
                CallLuaMethod(lateUpdate);
        }

        private void FixedUpdate()
        {
            if (markForLoadFromMachine)
            {
                Mod.LoadLuaRootFromMachine();
                markForLoadFromMachine = false;
            }

            if (!Machine.Active())
                return;

            if (hasErrors > 0)
                hasErrors--;

            if (scriptOk && (Machine.Active().SimulationMachine != null))
            {
                CallLuaMethod(fixedUpdateRef);

                foreach (AwaitAction awaitAction in awaitActions.ToArray())
                {
                    if (awaitAction.fixedUpdateTicksRemain <= 0)
                    {
                        awaitAction.action();
                        awaitActions.Remove(awaitAction);
                    } else
                    {
                        awaitAction.fixedUpdateTicksRemain--;
                    }
                }
            }
        }

        private int StoreLuaMethod(string name)
        {
            _lua.GetField(-1, name);
            if (!_lua.IsFunction(-1))
            {
                throw new Exception(string.Format(
                    "method {0} not found!", name));
            }
            return _lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
        }

        private void CallLuaMethod(int funcRef)
        {
            _lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, funcRef);

            // insert `traceback' function
            var b = _lua.GetTop();
            _lua.PushCSharpFunction(Traceback);
            _lua.Insert(b);

            var status = _lua.PCall(0, 0, b);
            if (status != ThreadStatus.LUA_OK)
            {
                scriptOk = false;
                Debug.LogError(_lua.ToString(-1));
            }

            // remove `traceback' function
            _lua.Remove(b);
        }

        private static int Traceback(ILuaState lua)
        {
            var msg = lua.ToString(1);
            if (msg != null)
            {
                lua.L_Traceback(lua, msg, 1);
            }
            // is there an error object?
            else if (!lua.IsNoneOrNil(1))
            {
                // try its `tostring' metamethod
                if (!lua.L_CallMeta(1, "__tostring"))
                {
                    lua.PushString("(no error message)");
                }
            }
            return 1;
        }
    }

    public class AwaitAction
    {
        public int fixedUpdateTicksRemain = 0;
        public Action action;
    }
}