
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
                new NameFuncPair("delta_time",    DeltaTime),
                new NameFuncPair("fixed_delta_time",    FixedUpdateTime),
                new NameFuncPair("time_scale",    TimeScale),   
            };

            lua.L_NewLib(define);
            return 1;
        }

        private static int OS_Clock(ILuaState lua)
        {
            lua.PushNumber(UnityEngine.Time.time);
            return 1;
        }
        private static int DeltaTime(ILuaState lua)
        {
            lua.PushNumber(UnityEngine.Time.deltaTime);
            return 1;
        }
        private static int FixedUpdateTime(ILuaState lua)
        {
            lua.PushNumber(UnityEngine.Time.fixedDeltaTime);
            return 1;
        }
        private static int TimeScale(ILuaState lua)
        {
            lua.PushNumber(UnityEngine.Time.timeScale);
            return 1;
        }
    }
}

