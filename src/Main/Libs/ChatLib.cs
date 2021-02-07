using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class ChatLib
    {
        public static List<int> chatListeners = new List<int>();

        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("add_listener", AddListener),
                new NameFuncPair("set_visible", SetVisible),
                new NameFuncPair("write_local", WriteLocal),
                new NameFuncPair("write_team", WriteTeam),
                new NameFuncPair("write_global", WriteGlobal),
                new NameFuncPair("clear", Clear),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int AddListener(ILuaState lua)
        {
            int fRef = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);
            chatListeners.Add(fRef);
            return 0;
        }

        private static int SetVisible(ILuaState lua)
        {
            LuaScripting.Instance.chatView?.SetVisibility(lua.ToBoolean(1));
            return 0;
        }

        private static int WriteLocal(ILuaState lua)
        {
            LuaScripting.Instance.chatView?.AddTextEntry(lua.L_CheckString(1));
            return 0;
        }

        private static int WriteTeam(ILuaState lua)
        {
            string str = lua.L_CheckString(1);
            if (LuaScripting.Instance.chatView)
                NetworkAuxAddPiece.Instance.SendSay(ChatMode.Team, str);
            return 0;
        }

        private static int WriteGlobal(ILuaState lua)
        {
            string str = lua.L_CheckString(1);
            if (LuaScripting.Instance.chatView)
                NetworkAuxAddPiece.Instance.SendSay(ChatMode.Global, str);
            return 0;
        }

        private static int Clear(ILuaState lua)
        {
            LuaScripting.Instance.chatView?.Clear();
            return 0;
        }
    }
}
