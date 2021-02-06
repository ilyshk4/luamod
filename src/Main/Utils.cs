using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting
{
    public static class Utils
    {
        public static CSharpFunctionDelegate CF(Func<bool> getBool)
        {
            return (lua) =>
            {
                lua.PushBoolean(getBool());
                return 1;
            };
        }

        public static CSharpFunctionDelegate CF(Func<float> getFloat)
        {
            return (lua) =>
            {
                lua.PushNumber(getFloat());
                return 1;
            };
        }

        public static CSharpFunctionDelegate CF(Func<Vector4> getVector)
        {
            return (lua) =>
            {
                Libs.VectorLib.PushVector(lua, getVector());
                return 1;
            };
        }

        public static CSharpFunctionDelegate CF(Func<string> getString)
        {
            return (lua) =>
            {
                lua.PushString(getString());
                return 1;
            };
        }

        public static NameFuncPair[] MakePretty(NameFuncPair[] define)
        {
            for (int i = 0; i < define.Length; i++)
            {
                for (int j = 1; j < define[i].Name.Length; j++)
                    if (char.IsUpper(define[i].Name[j]))
                    {
                        define[i].Name = define[i].Name.Insert(j, "_");
                        j++;
                    }
                define[i].Name = define[i].Name.ToLower();
            }

            return define;
        }

        public static int GetStableHashCode(string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
