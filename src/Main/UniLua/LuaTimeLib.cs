
namespace UniLua
{
    internal class LuaTimeLib
    {
        public const string LIB_NAME = "time";

        public static int OpenLib(ILuaState lua)
        {
            NameFuncPair[] define = new NameFuncPair[]
            {
                new NameFuncPair("time",    OS_Clock),
            };

            lua.L_NewLib(define);
            return 1;
        }

        private static int OS_Clock(ILuaState lua)
        {
            lua.PushNumber(UnityEngine.Time.time);
            return 1;
        }
    }
}

