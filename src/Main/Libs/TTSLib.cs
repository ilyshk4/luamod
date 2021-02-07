using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class TTSLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("speak", Speak),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int Speak(ILuaState lua)
        {
            Vector3 position = VectorLib.CheckVector(lua, 1);
            string lang = lua.L_CheckString(2);
            string text = lua.L_CheckString(3).Replace(' ', '+');

            

            return 0;
        }
    }
}
