using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class VectorLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("new", New),
                new NameFuncPair("distance", Distance),
                new NameFuncPair("dot", Dot),
                new NameFuncPair("lerp", Lerp),
                new NameFuncPair("lerp_unclamped", LerpUnclamped),
                new NameFuncPair("magnitude", Magnitude),
                new NameFuncPair("max", Max),
                new NameFuncPair("min", Min),
                new NameFuncPair("move_towards", MoveTowards),
                new NameFuncPair("normalize", Normalize),
                new NameFuncPair("project", Project),
                new NameFuncPair("scale", Scale),
                new NameFuncPair("add", Add),
                new NameFuncPair("subtract", Subtract),
                new NameFuncPair("negative", Negative),
                new NameFuncPair("multiply", Multiply),
                new NameFuncPair("equals", Equals), 
                new NameFuncPair("look_rotation", LookRotation),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int New(ILuaState lua)
        {
            PushVector(lua, new Vector4(lua.L_OptNumber(1, 0), lua.L_OptNumber(2, 0), lua.L_OptNumber(3, 0), lua.L_OptNumber(4, 0)));
            return 1;
        }

        private static int Distance(ILuaState lua)
        {
            lua.PushNumber(Vector4.Distance(CheckVector(lua, 1), CheckVector(lua, 2)));
            return 1;
        }

        private static int Dot(ILuaState lua)
        {
            lua.PushNumber(Vector4.Dot(CheckVector(lua, 1), CheckVector(lua, 2)));
            return 1;
        }

        private static int Lerp(ILuaState lua)
        {
            PushVector(lua, Vector4.Lerp(CheckVector(lua, 1), CheckVector(lua, 2), (float) lua.L_CheckNumber(3)));
            return 1;
        }

        private static int LerpUnclamped(ILuaState lua)
        {
            PushVector(lua, Vector4.LerpUnclamped(CheckVector(lua, 1), CheckVector(lua, 2), (float) lua.L_CheckNumber(3)));
            return 1;
        }

        private static int Magnitude(ILuaState lua)
        {
            lua.PushNumber(Vector4.Magnitude(CheckVector(lua, 1)));
            return 1;
        }

        private static int Max(ILuaState lua)
        {
            PushVector(lua, Vector4.Max(CheckVector(lua, 1), CheckVector(lua, 2)));
            return 1;
        }

        private static int Min(ILuaState lua)
        {
            PushVector(lua, Vector4.Min(CheckVector(lua, 1), CheckVector(lua, 2)));
            return 1;
        }

        private static int MoveTowards(ILuaState lua)
        {
            PushVector(lua, Vector4.MoveTowards(CheckVector(lua, 1), CheckVector(lua, 2), (float) lua.L_CheckNumber(3)));
            return 1;
        }

        private static int Normalize(ILuaState lua)
        {
            PushVector(lua, Vector4.Normalize(CheckVector(lua, 1)));
            return 1;
        }

        private static int Project(ILuaState lua)
        {
            PushVector(lua, Vector4.Project(CheckVector(lua, 1), CheckVector(lua, 2)));
            return 1;
        }

        private static int Scale(ILuaState lua)
        {
            PushVector(lua, Vector4.Scale(CheckVector(lua, 1), CheckVector(lua, 2)));
            return 1;
        }

        private static int Add(ILuaState lua)
        {
            PushVector(lua, CheckVector(lua, 1) + CheckVector(lua, 2));
            return 1;
        }

        private static int Subtract(ILuaState lua)
        {
            PushVector(lua, CheckVector(lua, 1) - CheckVector(lua, 2));
            return 1;
        }

        private static int Negative(ILuaState lua)
        {
            PushVector(lua, -CheckVector(lua, 1));
            return 1;
        }

        private static int Multiply(ILuaState lua)
        {
            PushVector(lua, CheckVector(lua, 1) * (float) lua.L_CheckNumber(2));
            return 1;
        }

        private static int Equals(ILuaState lua)
        {
            lua.PushBoolean(CheckVector(lua, 1) == CheckVector(lua, 2));
            return 1;
        }

        private static int LookRotation(ILuaState lua)
        {
            PushVector(lua, Quaternion.LookRotation(CheckVector(lua, 1)).eulerAngles);
            return 1;
        }

        public static void PushVector(ILuaState lua, Vector4 vector)
        {
            lua.NewTable();

            lua.PushNumber(vector.x);
            lua.SetField(-2, "x");

            lua.PushNumber(vector.y);
            lua.SetField(-2, "y");

            lua.PushNumber(vector.z);
            lua.SetField(-2, "z");

            lua.PushNumber(vector.w);
            lua.SetField(-2, "w");
        }

        public static Vector4 CheckVector(ILuaState lua, int index)
        {
            lua.GetField(index, "x");
            lua.GetField(index, "y");
            lua.GetField(index, "z");
            lua.GetField(index, "w");

            float x = (float) lua.L_CheckNumber(-4);
            float y = (float) lua.L_CheckNumber(-3);
            float z = (float) lua.L_CheckNumber(-2);
            float w = (float) lua.L_CheckNumber(-1);

            lua.Pop(4);

            return new Vector4(x, y, z, w);
        }
    }
}
