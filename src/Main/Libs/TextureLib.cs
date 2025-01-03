using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class TextureLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("from_base64", FromBase64),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int FromBase64(ILuaState lua)
        {
            string base64 = lua.L_CheckString(1);
            byte[] ba = Convert.FromBase64String(base64);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(ba);
            lua.PushInteger(LuaScripting.Instance.loadedTextures.Count);
            LuaScripting.Instance.loadedTextures.Add(tex);
            return 1;
        }
    }
}
