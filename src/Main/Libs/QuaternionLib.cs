using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class QuaternionLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("angle", Angle),
                new NameFuncPair("angle_axis", AngleAxis),
                new NameFuncPair("dot", Dot),
                new NameFuncPair("new", Euler),
                new NameFuncPair("euler", Euler),
                new NameFuncPair("from_to_rotation", FromToRotation),
                new NameFuncPair("inverse", Inverse),
                new NameFuncPair("lerp", Lerp),
                new NameFuncPair("lerp_unclamped", LerpUnclamped),
                new NameFuncPair("slerp", Slerp),
                new NameFuncPair("slerp_unclamped", SlerpUnclamped),
                new NameFuncPair("look_rotation", LookRotation),
                new NameFuncPair("rotate_towards", RotateTowards),
                new NameFuncPair("multiply", Multiply),
                new NameFuncPair("multiply_on_vector", MultiplyOnVector),
                new NameFuncPair("equals", Equals),
            };

            lua.L_NewLib(define);

            return 1;
        }

        public static Quaternion CheckQuat(ILuaState lua, int index)
        {
            Vector4 vec = VectorLib.CheckVector(lua, index);
            Quaternion quat = new Quaternion
            {
                x = vec.x,
                y = vec.y,
                z = vec.z,
                w = vec.w
            };
            return quat;
        }

        public static Quaternion OptQuat(ILuaState lua, int index, Quaternion def)
        {
            LuaType t = lua.Type(index);
            if (t == LuaType.LUA_TNONE ||
                t == LuaType.LUA_TNIL)
            {
                return def;
            }
            else
            {
                return CheckQuat(lua, index);
            }
        }

        public static void PushQuat(ILuaState lua, Quaternion quat)
        {
            VectorLib.PushVector(lua, new Vector4(quat.x, quat.y, quat.z, quat.w));
        }

        private static int Angle(ILuaState lua)
        {
            lua.PushNumber(Quaternion.Angle(CheckQuat(lua, 1), CheckQuat(lua, 2)));
            return 1;
        }

        private static int AngleAxis(ILuaState lua)
        {
            PushQuat(lua, Quaternion.AngleAxis((float) lua.L_CheckNumber(1), VectorLib.CheckVector(lua, 2)));
            return 1;
        }

        private static int Dot(ILuaState lua)
        {
            lua.PushNumber(Quaternion.Dot(CheckQuat(lua, 1), CheckQuat(lua, 2)));
            return 1;
        }

        private static int Euler(ILuaState lua)
        {
            if (lua.GetTop() == 1 && lua.Type(1) == LuaType.LUA_TTABLE)
                PushQuat(lua, Quaternion.Euler(VectorLib.CheckVector(lua, 1)));
            else
                PushQuat(lua, Quaternion.Euler(lua.L_OptNumber(1, 0), lua.L_OptNumber(2, 0), lua.L_OptNumber(3, 0)));

            return 1;
        }

        private static int FromToRotation(ILuaState lua)
        {
            PushQuat(lua, Quaternion.FromToRotation(VectorLib.CheckVector(lua, 1), VectorLib.CheckVector(lua, 2)));
            return 1;
        }

        private static int Inverse(ILuaState lua)
        {
            PushQuat(lua, Quaternion.Inverse(CheckQuat(lua, 1)));
            return 1;
        }

        private static int Lerp(ILuaState lua)
        {
            PushQuat(lua, Quaternion.Lerp(CheckQuat(lua, 1), CheckQuat(lua, 2), (float) lua.L_CheckNumber(3)));
            return 1;
        }

        private static int LerpUnclamped(ILuaState lua)
        {
            PushQuat(lua, Quaternion.LerpUnclamped(CheckQuat(lua, 1), CheckQuat(lua, 2), (float)lua.L_CheckNumber(3)));
            return 1;
        }
        private static int Slerp(ILuaState lua)
        {
            PushQuat(lua, Quaternion.Slerp(CheckQuat(lua, 1), CheckQuat(lua, 2), (float)lua.L_CheckNumber(3)));
            return 1;
        }

        private static int SlerpUnclamped(ILuaState lua)
        {
            PushQuat(lua, Quaternion.SlerpUnclamped(CheckQuat(lua, 1), CheckQuat(lua, 2), (float)lua.L_CheckNumber(3)));
            return 1;
        }

        private static int LookRotation(ILuaState lua)
        {
            PushQuat(lua, Quaternion.LookRotation(VectorLib.CheckVector(lua, 1), VectorLib.CheckVector(lua, 2)));
            return 1;
        }

        private static int RotateTowards(ILuaState lua)
        {
            PushQuat(lua, Quaternion.RotateTowards(CheckQuat(lua, 1), CheckQuat(lua, 2), (float) lua.L_CheckNumber(3)));
            return 1;
        }

        private static int Multiply(ILuaState lua)
        {
            PushQuat(lua, CheckQuat(lua, 1) * CheckQuat(lua, 2));
            return 1;
        }

        private static int MultiplyOnVector(ILuaState lua)
        {
            VectorLib.PushVector(lua, CheckQuat(lua, 1) * VectorLib.CheckVector(lua, 2));
            return 1;
        }
        
        private static int Equals(ILuaState lua)
        {
            lua.PushBoolean(CheckQuat(lua, 1) == CheckQuat(lua, 2));
            return 1;
        }
    }
}
