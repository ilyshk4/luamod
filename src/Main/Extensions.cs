using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting
{
    public static class Extensions
    {
        public static float L_OptNumber(this ILuaState lua, int a, double def)
        {
            LuaType t = lua.Type(a);
            if (t == LuaType.LUA_TNONE ||
                t == LuaType.LUA_TNIL)
            {
                return (float) def;
            }
            else
            {
                return (float) lua.L_CheckNumber(a);
            }
        }

        public static int ReturnError(this ILuaState lua, int index, string errorMsg)
        {
            lua.PushString(errorMsg);
            return lua.L_ArgError(index, errorMsg);
        }

        public static string[] CheckStringArray(this ILuaState lua, int index)
        {
            lua.L_CheckType(index, LuaType.LUA_TTABLE);
            string[] arr = new string[lua.L_Len(index)];

            for (int i = 1; i <= lua.L_Len(index); i++)
            {
                lua.RawGetI(index, i);
                arr[i - 1] = lua.L_CheckString(-1);
            }

            return arr;
        }

        // 2hard4me2understandStack
        public static Vector4[] CheckVectorArray(this ILuaState lua, int index)
        {
            lua.L_CheckType(index, LuaType.LUA_TTABLE);
            Vector4[] arr = new Vector4[lua.L_Len(index)];

            for (int i = 1; i <= lua.L_Len(index); i++)
            {
                lua.RawGetI(index, i);
                arr[i - 1] = Libs.VectorLib.CheckVector(lua, -1);
            }

            return arr;
        }
    }
}
