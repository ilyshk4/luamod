using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class ScreenLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("width", Utils.CF(() => Screen.width)),
                new NameFuncPair("height", Utils.CF(() => Screen.height)),
                new NameFuncPair("fullscreen", Utils.CF(() => Screen.fullScreen)),
                new NameFuncPair("dpi", Utils.CF(() => Screen.dpi)),
            };

            lua.L_NewLib(define);

            return 1;
        }
    }
}
