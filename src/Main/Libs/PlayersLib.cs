using Modding.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class PlayersLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("count", Count),
                new NameFuncPair("get", Get),
                new NameFuncPair("by_id", ById),
                new NameFuncPair("get_all", GetAll),
            };

            lua.L_NewLib(define);

            return 1;
        }

        public static void PushPlayerInfo(ILuaState lua, Player player)
        {
            lua.NewTable();

            lua.PushCSharpFunction(Utils.CF(() => player.InLocalSim));
            lua.SetField(-2, "in_local_sim");

            lua.PushCSharpFunction(Utils.CF(() => player.IsHost));
            lua.SetField(-2, "is_host");

            lua.PushCSharpFunction(Utils.CF(() => player.IsLocalPlayer));
            lua.SetField(-2, "is_local_player");

            lua.PushCSharpFunction(Utils.CF(() => player.IsSpectator));
            lua.SetField(-2, "is_spectator");

            lua.PushCSharpFunction(Utils.CF(() => player.Machine.SimulationMachine != null));
            lua.SetField(-2, "is_simulating");

            lua.PushCSharpFunction(Utils.CF(() => player.Name));
            lua.SetField(-2, "name");

            lua.PushCSharpFunction(Utils.CF(() => player.NetworkId));
            lua.SetField(-2, "id");

            lua.PushCSharpFunction(Utils.CF(() => (int) player.Team));
            lua.SetField(-2, "team");

            lua.PushCSharpFunction((ll) =>
            {
                MachineLib.PushMachineInfo(ll, player.Machine.InternalObject);
                return 1;
            });
            lua.SetField(-2, "get_machine_info");
        }

        private static int Count(ILuaState lua)
        {
            lua.PushInteger(Player.GetAllPlayers().Count);
            return 1;
        }

        private static int Get(ILuaState lua)
        {
            if (lua.IsString(1))
            {
                Player pl = Player.GetAllPlayers().Find(p => p.Name == lua.L_CheckString(1));

                if (pl != null)
                {
                    PushPlayerInfo(lua, pl);
                    return 1;
                }
            }
            
            int n = lua.L_CheckInteger(1);
            if (n < Player.GetAllPlayers().Count)
            {
                Player pl = Player.GetAllPlayers().ElementAt(n);

                if (pl != null)
                {
                    PushPlayerInfo(lua, pl);
                    return 1;
                }
            }

            lua.PushNil();
            return 1;
        }

        private static int ById(ILuaState lua)
        {
            int n = lua.L_CheckInteger(1);
            Player pl = Player.From((ushort) n);

            if (pl != null)
            {
                PushPlayerInfo(lua, pl);
                return 1;
            }

            lua.PushNil();
            return 1;
        }

        private static int GetAll(ILuaState lua)
        {
            lua.NewTable();

            List<Player> players = Player.GetAllPlayers();

            for (int i = 0; i < players.Count; i++)
            {
                PushPlayerInfo(lua, players[i]);
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }
    }
}
