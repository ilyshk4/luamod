using Modding;
using Modding.Blocks;
using spaar.ModLoader.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private Rect windowRect = new Rect(6, 70, 200, 254);
        private Rect consoleWindowRect = new Rect(274, 70, 500, 600);
        private Vector2 consoleWindowScrollPos = Vector2.zero;

        private int startRef, updateRef, lateUpdate, fixedUpdateRef, onguiRef;
        private bool scriptOk;
        private static bool markForLoadFromMachine = true;

        public List<AwaitAction> awaitActions = new List<AwaitAction>();

        public Dictionary<string, List<Block>> localMachineBlockRefs = new Dictionary<string, List<Block>>();
        public Dictionary<KeyCode, List<Block>> localMachineUsedKeys = new Dictionary<KeyCode, List<Block>>();

        public List<Texture2D> loadedTextures = new List<Texture2D>();

        public Queue<string> printLog = new Queue<string>();

        public Camera hudCamera;
        public Camera mainCamera;

        public Font besiegeFont;
        public ChatView chatView;
        public Transform chatViewContent;
        private int lastChatMsgHashCode;

        public static void AddAwaitAction(Action action, int time)
        {
            Instance.awaitActions.Add(new AwaitAction()
            {
                action = action,
                fixedUpdateTicksRemain = time
            });
        }

        public static void Init()
        {
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
                        Instance.loadedTextures.Clear();
                        Instance.awaitActions.Clear();
                        Instance.localMachineBlockRefs.Clear();
                        Instance.localMachineUsedKeys.Clear();
                        Libs.ChatLib.chatListeners.Clear();

                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    } else
                    {
                        Instance.printLog.Clear();
                    }
                }
            };

            Events.OnMachineDestroyed += () =>
            {
                Machine.Active().MachineData.Clear();
            };

            Events.OnBlockInit += OnBlockInit;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (a, b) =>
            {
                Instance.Start();
            };
        }

        private static void OnBlockInit(Block block)
        {
            if (block.InternalObject.BuildIndex == 0)
            {
                if (block.Machine.SimulationMachine)
                    block.Machine.SimulationMachine.gameObject.AddComponent<LuaPlayerMachine>().machine = block.InternalObject.ParentMachine;

                if (block.InternalObject.ParentMachine == Machine.Active())
                {
                    if (!Machine.Active().SimulationMachine)
                    {
                        markForLoadFromMachine = true;
                    }
                    else
                    {
                        foreach (Block b in block.Machine.BuildingBlocks)
                        {
                            MText refKey = b.InternalObject.GetMapperType("bmt-lua_ref_key") as MText;

                            if (refKey != null)
                            {
                                if (!Instance.localMachineBlockRefs.ContainsKey(refKey.Value))
                                    Instance.localMachineBlockRefs.Add(refKey.Value, new List<Block>());
                                Instance.localMachineBlockRefs[refKey.Value].Add(b);
                            }

                            foreach (MapperType mt in b.InternalObject.MapperTypes)
                                if (mt is MKey)
                                {
                                    MKey mkey = mt as MKey;
                                    for (int i = 0; i < mkey.KeysCount; i++)
                                    {
                                        KeyCode keyCode = mkey.GetKey(i);
                                        if (!Instance.localMachineUsedKeys.ContainsKey(keyCode))
                                            Instance.localMachineUsedKeys.Add(keyCode, new List<Block>());
                                        Instance.localMachineUsedKeys[keyCode].Add(b);
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
            _lua.L_RequireF("chat", Libs.ChatLib.OpenLib, true);
            _lua.L_RequireF("quaternion", Libs.QuaternionLib.OpenLib, true);
            _lua.L_RequireF("texture", Libs.TextureLib.OpenLib, true);
            _lua.L_RequireF("game", Libs.GameLib.OpenLib, true);
            _lua.L_RequireF("shapes", Libs.ShapesLib.OpenLib, true);
            _lua.L_RequireF("entities", Libs.EntitiesLib.OpenLib, true);

            _status = _lua.L_DoFile("main.lua");

            if (_status != ThreadStatus.LUA_OK)
            {
                string s = _lua.ToString(-1);
                Debug.LogError(s); LuaScripting.Instance.AddPrintLog(s);
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
            if (!Elements.IsInitialized)
                Elements.RebuildElements();

            GUI.skin = Mod.Skin;

            if (!Machine.Active())
                return;

            if (SHOW_GUI)
            {
                windowRect = GUI.Window(84475229, windowRect, DoWindow, "Lua Scripting");

                if (printLog.Count > 0)
                    consoleWindowRect = GUI.Window(534754242, consoleWindowRect, ConsoleWindow, "Log");

                if (BlockMapper.CurrentInstance && BlockMapper.CurrentInstance.IsBlock && BlockMapper.CurrentInstance.Block.MapperTypes.Count > 0)
                {
                    Rect inspectorRect = windowRect;
                    inspectorRect.width = 300;
                    inspectorRect.x = windowRect.x + windowRect.width + 10;
                    inspectorRect.height = 84 + BlockMapper.CurrentInstance.Block.MapperTypes.Count * 30;
                    GUI.Window(84475239, inspectorRect, InspectKeysWindow, "Mapper Keys");
                }
            }

            if (scriptOk && (Machine.Active().SimulationMachine != null))
                CallLuaMethod(onguiRef);
        }

        private void InspectKeysWindow(int uselessId)
        {
            int i = 1;
            GUI.Label(new Rect(10, 30 + 24, 135, 20), "Name");
            GUI.Label(new Rect(155, 30 + 24, 135, 20), "Key");
            foreach (MapperType mt in BlockMapper.CurrentInstance.Block.MapperTypes)
            {
                GUI.Label(new Rect(10, 30 + 30 * i + 24, 135, 20), $"{mt.DisplayName}");
                GUI.Label(new Rect(155, 30 + 30 * i + 24, 135, 20), $"{mt.Key}");
                i++;
            }
        }

        private void DoGroupRename(int uselessId)
        {

        }

        private void DoWindow(int uselessId)
        {
            if (GUI.Button(new Rect(10, 24 + 30, 180, 30), "Open LuaRoot Folder"))
            {
                if (!ModIO.ExistsDirectory("LuaRoot"))
                    ModIO.CreateDirectory("LuaRoot");
                ModIO.OpenFolderInFileBrowser("LuaRoot");
            }

            if (GUI.Button(new Rect(10, 24 + 70, 180, 30), "Save LuaRoot Manually"))
            {
                Mod.SaveLuaRootToMachine();
            }

            if (GUI.Button(new Rect(10, 24 + 110, 180, 30), "Load LuaRoot Manually"))
            {
                Mod.LoadLuaRootFromMachine();
            }

            if (GUI.Button(new Rect(10, 24 + 150, 180, 30), "Online Docs"))
            {
                Application.OpenURL("https://github.com/ilyshk4/luamod-wiki/wiki");
            }

            if (GUI.Button(new Rect(10, 24 + 190, 180, 30), "Offline Docs"))
            {
                ModIO.OpenFolderInFileBrowser("Doc");
            }

            GUI.DragWindow();
        }

        private void ConsoleWindow(int uslsId)
        {
            string result = string.Join("\n", printLog.Reverse().ToArray());
            consoleWindowScrollPos = GUI.BeginScrollView(new Rect(10, 54, 500 - 10 - 10, 600 - 54 - 10), consoleWindowScrollPos, new Rect(0, 0, 500 - 40, printLog.Count * 18));
            GUI.TextArea(new Rect(0, 0, 500 - 20, printLog.Count * 18), result);
            GUI.EndScrollView();
            GUI.DragWindow();
        }

        private void Start()
        {
            try
            {
                mainCamera = Camera.main;
                hudCamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();
                chatView = FindObjectOfType<ChatView>();
                chatViewContent = chatView?.transform?.GetChild(0)?.GetChild(0)?.GetChild(0)?.GetChild(0)?.GetChild(0)?.GetChild(0);
                besiegeFont = FindObjectOfType<DynamicText>().font;
            } catch (Exception)
            {

            }
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
                markForLoadFromMachine = false;
                Mod.LoadLuaRootFromMachine();
            }

            if (!Machine.Active())
                return;

            if (scriptOk && (Machine.Active().SimulationMachine != null))
            {
                var chatMsg = chatViewContent?.GetChild(chatViewContent.childCount - 1)?.GetComponent<UnityEngine.UI.Text>();
                if (chatMsg != null)
                {
                    if (chatMsg.GetHashCode() != lastChatMsgHashCode && chatMsg.text.Contains(":  "))
                    {
                        string[] sp = chatMsg.text.Split(new string[] { ":  " }, StringSplitOptions.None);
                        if (sp.Length == 2)
                        {
                            string nick = Regex.Replace(sp[0], "<.*?>", String.Empty);
                            string text = Regex.Replace(sp[1], "<.*?>", String.Empty);
                            foreach (int fRef in Libs.ChatLib.chatListeners)
                            {
                                _lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, fRef);

                                _lua.PushString(nick);
                                _lua.PushString(text);

                                if (_lua.PCall(2, 0, 0) != ThreadStatus.LUA_OK)
                                {
                                    Debug.LogError(_lua.ToString(-1));
                                    LuaScripting.Instance.AddPrintLog(_lua.ToString(-1));
                                }
                            }
                        }
                    }
                    lastChatMsgHashCode = chatMsg.GetHashCode();
                }

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

        internal void AddPrintLog(string v)
        {
            if (printLog.Count > 300)
                printLog.Dequeue();
            printLog.Enqueue(v);
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
                Debug.LogError(_lua.ToString(-1)); LuaScripting.Instance.AddPrintLog(_lua.ToString(-1));
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
