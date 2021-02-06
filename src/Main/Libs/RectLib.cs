using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class RectLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("new", New),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int New(ILuaState lua)
        {
            PushRect(lua, new Rect(lua.L_OptInt(1, 0), lua.L_OptInt(2, 0), lua.L_OptInt(3, 0), lua.L_OptInt(4, 0)));

            return 1;
        }

        public static void PushRect(ILuaState lua, Rect rect)
        {
            lua.NewTable();

            lua.PushNumber(rect.x);
            lua.SetField(-2, "x");

            lua.PushNumber(rect.y);
            lua.SetField(-2, "y");

            lua.PushNumber(rect.width);
            lua.SetField(-2, "width");

            lua.PushNumber(rect.height);
            lua.SetField(-2, "height");
        }

        public static Rect CheckRect(ILuaState lua, int index)
        {
            lua.GetField(index, "x");
            lua.GetField(index, "y");
            lua.GetField(index, "width");
            lua.GetField(index, "height");

            float x = (float) lua.L_CheckNumber(-4);
            float y = (float) lua.L_CheckNumber(-3);
            float width = (float) lua.L_CheckNumber(-2);
            float height = (float) lua.L_CheckNumber(-1);

            lua.Pop(4);

            return new Rect(x, y, width, height);
        }

        //public static Rect L_CheckRect(ILuaState lua)
        //{
        //
        //}
    }
}
