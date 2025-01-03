using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class GameLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("memory", Memory),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int Memory(ILuaState lua)
        {
            lua.PushNumber(GC.GetTotalMemory(false));
            return 1;
        }
    }
}
