using Modding;
using Modding.Blocks;
using Modding.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class MachineLib
    {
        public static Dictionary<KeyCode, bool> keyEmulators = new Dictionary<KeyCode, bool>();
        public static Dictionary<string, KeyCode> keyCodes = new Dictionary<string, KeyCode>();

        public static NameFuncPair[] define;

        public static void Init()
        {
            if (keyCodes.Count == 0)
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    string key = keyCode.ToString().ToLower();
                    if (!keyCodes.Keys.Contains(key))
                        keyCodes.Add(key, keyCode);
                }

            Events.OnMachineSimulationToggle += (pl, state) =>
            {
                if (pl.Player == null || (pl.Player != null && pl.Player.IsLocalPlayer))
                {
                    if (!state)
                    {
                        keyEmulators.Clear();
                    }
                }
            };

            define = new NameFuncPair[]
            {
                new NameFuncPair("new_key_emulator", NewKeyEmulator),
                new NameFuncPair("get_refs_control", GetRefsControl),
                new NameFuncPair("get_machine_info", GetMachineInfo),
            };
        }

        public static int OpenLib(ILuaState lua)
        {
            lua.L_NewLib(define);

            return 1;
        }

        private static int GetBlockInfo(ILuaState lua, Machine machine)
        {
            if (lua.Type(1) == LuaType.LUA_TSTRING)
            {
                if (machine == Machine.Active())
                {
                    string key = lua.L_CheckString(1);
                    int index = lua.L_OptInt(2, 0);

                    if (LuaScripting.Instance.localMachineBlockRefs.TryGetValue(key, out List<Block> b))
                    {
                        if (index < b.Count)
                        {
                            PushBlockInfo(lua, b[index].SimBlock.InternalObject);
                            return 1;
                        }
                        else
                        {
                            return lua.ReturnError(2, $"there is no block with index {index} in the list of all blocks with that reference key");
                        }
                    }
                    return lua.ReturnError(1, $"there is no block with reference key {key}");
                }
                else return lua.ReturnError(1, $"you can`t get block info by reference key from non local machine, use get_block_info(int build_index) instead.");
            }

            if (lua.Type(1) == LuaType.LUA_TNUMBER)
            {
                int index = lua.L_CheckInteger(1);

                if (index < machine.SimulationBlocks.Count)
                {
                    PushBlockInfo(lua, machine.SimulationBlocks[index]);
                    return 1;
                }
                else return lua.ReturnError(1, $"build index {index} out of bounds {machine.SimulationBlocks.Count}");
            }

            return 0;
        }

        public static void PushMachineInfo(ILuaState lua, Machine machine)
        {
            LuaPlayerMachine playerMachine = machine.SimulationMachine.GetComponent<LuaPlayerMachine>();
            lua.NewTable();

            lua.PushCSharpFunction((ll) =>
            {
                return GetBlockInfo(ll, machine);
            });
            lua.SetField(-2, "get_block_info");

            lua.PushCSharpFunction(Utils.CF(() => machine.BlockCount));
            lua.SetField(-2, "block_count");

            lua.PushCSharpFunction(Utils.CF(() => machine.ClusterCount));
            lua.SetField(-2, "cluster_count");

            lua.PushCSharpFunction(Utils.CF(() => machine.MachineCenterPos));
            lua.SetField(-2, "center");

            lua.PushCSharpFunction(Utils.CF(() => machine.Mass));
            lua.SetField(-2, "mass");

            lua.PushCSharpFunction(Utils.CF(() => machine.MiddlePosition));
            lua.SetField(-2, "middle");

            lua.PushCSharpFunction(Utils.CF(() => machine.Name));
            lua.SetField(-2, "name");

            lua.PushCSharpFunction(Utils.CF(() => machine.PlayerID));
            lua.SetField(-2, "player_id");

            lua.PushCSharpFunction((ll) =>
            {
                PlayersLib.PushPlayerInfo(ll, Player.From(machine.PlayerID));
                return 1;
            });
            lua.SetField(-2, "player");

            lua.PushCSharpFunction(Utils.CF(() => machine.SimulationBlocks[0].transform.position));
            lua.SetField(-2, "position");

            lua.PushCSharpFunction(Utils.CF(() => machine.SimulationBlocks[0].transform.rotation * Vector3.forward));
            lua.SetField(-2, "rotation");

            lua.PushCSharpFunction(Utils.CF(() => playerMachine.blockInfos[0].velocity));
            lua.SetField(-2, "velocity");

            lua.PushCSharpFunction(Utils.CF(() => playerMachine.blockInfos[0].angularVelocity));
            lua.SetField(-2, "angular_velocity");

            lua.PushCSharpFunction(Utils.CF(() => machine.Size));
            lua.SetField(-2, "size");

            lua.PushCSharpFunction(Utils.CF(() => machine.UnbreakableMode));
            lua.SetField(-2, "unbreakable");

            lua.PushCSharpFunction(Utils.CF(() => machine.InfiniteAmmoMode));
            lua.SetField(-2, "infinite_ammo");

            lua.PushCSharpFunction(Utils.CF(() => machine.IsDraggingBlocks));
            lua.SetField(-2, "is_dragging_blocks");

            lua.PushCSharpFunction(Utils.CF(() => (int)machine.FirstBlock.Team));
            lua.SetField(-2, "team");

            lua.PushCSharpFunction(Utils.CF(() => machine.SimulationMachine != null));
            lua.SetField(-2, "is_simulating");
        }

        private static int GetMachineInfo(ILuaState lua)
        {
            PushMachineInfo(lua, Machine.Active());
            return 1;
        }

        private static int NewKeyEmulator(ILuaState lua)
        {
            KeyCode keyCode = keyCodes[lua.L_CheckString(1).ToLower()];

            if (keyEmulators.ContainsKey(keyCode))
            {
                return lua.ReturnError(1, "there is already a key emulator with same keycode");
            }

            keyEmulators.Add(keyCode, false);

            CSharpFunctionDelegate start = (state) =>
            {
                if (!keyEmulators[keyCode])
                {
                    Emulate(keyCode, true);
                    keyEmulators[keyCode] = true;
                }
                return 1;
            };

            CSharpFunctionDelegate stop = (state) =>
            {
                if (keyEmulators[keyCode])
                {
                    Emulate(keyCode, false);
                    keyEmulators[keyCode] = false;
                }
                return 1;
            };

            CSharpFunctionDelegate click = (state) =>
            {
                start(state);
                LuaScripting.AddAwaitAction(() => stop(state), 2);
                return 1;
            };

            CSharpFunctionDelegate active = (state) =>
            {
                state.PushBoolean(keyEmulators[keyCode]);
                return 1;
            };

            lua.NewTable(); //4, 1);

            lua.PushCSharpFunction(start);
            lua.SetField(-2, "start");

            lua.PushCSharpFunction(stop);
            lua.SetField(-2, "stop");

            lua.PushCSharpFunction(click);
            lua.SetField(-2, "click");

            lua.PushCSharpFunction(active);
            lua.SetField(-2, "active");

            return 1;
        }

        private static int GetRefsControl(ILuaState lua)
        {
            string key = lua.L_CheckString(1);

            if (!LuaScripting.Instance.localMachineBlockRefs.ContainsKey(key))
            {
                return lua.ReturnError(0, "there is no block with key: " + key);
            }

            List<Block> blocks = LuaScripting.Instance.localMachineBlockRefs[key];

            CSharpFunctionDelegate setSliderValue = (state) =>
            {
                string mapperKey = lua.L_CheckString(1);
                foreach (Block b in blocks)
                {
                    string uk = b.InternalObject.BuildIndex + mapperKey;
                    float value = (float)lua.L_CheckNumber(2);

                    MapperType mapperType = b.InternalObject.SimBlock.GetMapperType("bmt-" + mapperKey);
                    if (mapperType == null)
                        mapperType = b.InternalObject.SimBlock.GetMapperType(mapperKey);

                    if (mapperType == null)
                    {
                        ModNetworking.SendToHost(Mod.netMsgSliderValue.CreateMessage(new object[]
                        {
                            b, Utils.GetStableHashCode(mapperKey), value
                        }));
                    }
                    else
                    {
                        MSlider slider = mapperType as MSlider;
                        slider.SetValue(value);
                        slider.ApplyValue();
                    }
                }
                return 0;
            };

            CSharpFunctionDelegate setSteering = (state) =>
            {
                float angle = (float)lua.L_CheckNumber(1);

                foreach (Block b in blocks)
                {
                    if (b.InternalObject is SteeringWheel)
                    {
                        SteeringWheel steeringWheel = b?.SimBlock?.InternalObject as SteeringWheel;

                        if (steeringWheel != null)
                        {
                            if (!steeringWheel.noRigidbody)
                            {
                                steeringWheel.AngleToBe = angle;
                            }
                            else
                            {
                                ModNetworking.SendToHost(Mod.netMsgSetSteering.CreateMessage(new object[]
                                {
                                    b, angle
                                }));
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"[LuaScripting] There is no steering wheel on {b}.");
                    }
                }
                return 0;
            };

            lua.NewTable(); //2, 1);

            lua.PushCSharpFunction(setSliderValue);
            lua.SetField(-2, "set_slider");

            lua.PushCSharpFunction(setSteering);
            lua.SetField(-2, "set_steering");
            return 1;
        }

        public static void PushBlockInfo(ILuaState lua, BlockBehaviour block)
        {
            LuaPlayerMachine playerMachine = block.ParentMachine.SimulationMachine.GetComponent<LuaPlayerMachine>();

            lua.NewTable();

            lua.PushCSharpFunction(Utils.CF(() => block.transform.position));
            lua.SetField(-2, "position");

            lua.PushCSharpFunction(Utils.CF(() => block.transform.forward));
            lua.SetField(-2, "forward");

            lua.PushCSharpFunction(Utils.CF(() => block.transform.right));
            lua.SetField(-2, "right");

            lua.PushCSharpFunction(Utils.CF(() => block.transform.up));
            lua.SetField(-2, "up");

            lua.PushCSharpFunction(Utils.CF(() => block.transform.rotation.eulerAngles));
            lua.SetField(-2, "rotation");

            lua.PushCSharpFunction(Utils.CF(() => block.BeingVacuumed));
            lua.SetField(-2, "being_vacuumed");

            lua.PushCSharpFunction(Utils.CF(() => block.BlockID));
            lua.SetField(-2, "id");

            lua.PushCSharpFunction(Utils.CF(() => block.BuildIndex));
            lua.SetField(-2, "build_index");

            lua.PushCSharpFunction(Utils.CF(() => block.BlockHealth.health));
            lua.SetField(-2, "health");

            lua.PushCSharpFunction(Utils.CF(() => block.fireTag.burning));
            lua.SetField(-2, "burning");

            lua.PushCSharpFunction(Utils.CF(() => block.Flipped));
            lua.SetField(-2, "flipped");

            lua.PushCSharpFunction(Utils.CF(() => block.iceTag.frozen));
            lua.SetField(-2, "frozen");

            lua.PushCSharpFunction(Utils.CF(() => block.InWind));
            lua.SetField(-2, "in_wind");

            lua.PushCSharpFunction(Utils.CF(() => block.IsDestroyed));
            lua.SetField(-2, "destroyed");

            lua.PushCSharpFunction(Utils.CF(() => block.isZeroG));
            lua.SetField(-2, "zero_g");

            lua.PushCSharpFunction(Utils.CF(() => block.originalMass));
            lua.SetField(-2, "original_mass");

            lua.PushCSharpFunction(Utils.CF(() => block.Scale));
            lua.SetField(-2, "scale");

            lua.PushCSharpFunction(Utils.CF(() => playerMachine.blockInfos[block.BuildIndex].velocity));
            lua.SetField(-2, "velocity");

            lua.PushCSharpFunction(Utils.CF(() => playerMachine.blockInfos[block.BuildIndex].angularVelocity));
            lua.SetField(-2, "angular_velocity");
        }

        private static void Emulate(KeyCode key, bool emulate)
        {
            if (LuaScripting.Instance.localMachineUsedKeys.Keys.Contains(key))
                foreach (Block block in LuaScripting.Instance.localMachineUsedKeys[key])
                {
                    foreach (MapperType mapperType in block.SimBlock.InternalObject.MapperTypes)
                        if (mapperType is MKey)
                        {
                            MKey mkey = mapperType as MKey;
                            if (mkey.HasKey(key))
                                mkey.UpdateEmulation(emulate);
                        }

                    ModNetworking.SendToAll(Mod.netMsgEmulateKey.CreateMessage(new object[] {
                        block, (int) key, emulate
                    }));
                }
        }
    }
}
