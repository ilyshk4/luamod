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
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int Count(ILuaState lua)
        {
            lua.PushInteger(Modding.Common.Player.GetAllPlayers().Count);
            return 1;
        }
    }
}
