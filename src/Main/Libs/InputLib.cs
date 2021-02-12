using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniLua;

namespace LuaScripting.Libs
{
    public static class InputLib
    {
        public static NameFuncPair[] define;

        public static void Init()
        {
            define = new NameFuncPair[]
            {
                new NameFuncPair("mouse_screen_position", Utils.CF(() => Input.mousePosition)),
                new NameFuncPair("mouse_raycast_hit_point", MouseRaycasyHitPoint),
                new NameFuncPair("get_axis", GetAxis),
                new NameFuncPair("get_axis_raw", GetAxisRaw),
                new NameFuncPair("get_key", GetKey),
                new NameFuncPair("get_key_down", GetKeyDown),
                new NameFuncPair("get_key_up", GetKeyUp),
                new NameFuncPair("get_mouse_button", GetMouseButton),
                new NameFuncPair("get_mouse_button_down", GetMouseButtonDown),
                new NameFuncPair("get_mouse_button_up", GetMouseButtonUp),
                new NameFuncPair("any_key", Utils.CF(() => Input.anyKey)),
                new NameFuncPair("any_key_down", Utils.CF(() => Input.anyKeyDown)),
            };
        }

        public static int OpenLib(ILuaState lua)
        {
            lua.L_NewLib(define);

            return 1;
        }

        public static int MouseRaycasyHitPoint(ILuaState lua)
        {
            RaycastHit hit;
            Ray ray = LuaScripting.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                VectorLib.PushVector(lua, hit.point);
                return 1;
            }
            VectorLib.PushVector(lua, Vector4.zero);
            return 1;
        }

        public static int GetAxis(ILuaState lua)
        {
            lua.PushNumber(Input.GetAxis(lua.L_CheckString(1)));
            return 1;
        }
        public static int GetAxisRaw(ILuaState lua)
        {   
            lua.PushNumber(Input.GetAxisRaw(lua.L_CheckString(1)));
            return 1;
        }

        public static int GetKey(ILuaState lua)
        {
            lua.PushBoolean(Input.GetKey(MachineLib.keyCodes[lua.L_CheckString(1).ToLower()]));
            return 1;
        }
        public static int GetKeyDown(ILuaState lua)
        {
            lua.PushBoolean(Input.GetKeyDown(MachineLib.keyCodes[lua.L_CheckString(1).ToLower()]));
            return 1;
        }
        public static int GetKeyUp(ILuaState lua)
        {
            lua.PushBoolean(Input.GetKeyUp(MachineLib.keyCodes[lua.L_CheckString(1).ToLower()]));
            return 1;
        }

        public static int GetMouseButton(ILuaState lua)
        {
            lua.PushBoolean(Input.GetMouseButton(lua.L_CheckInteger(1)));
            return 1;
        }
        public static int GetMouseButtonDown(ILuaState lua)
        {
            lua.PushBoolean(Input.GetMouseButtonDown(lua.L_CheckInteger(1)));
            return 1;
        }
        public static int GetMouseButtonUp(ILuaState lua)
        {
            lua.PushBoolean(Input.GetMouseButtonUp(lua.L_CheckInteger(1)));
            return 1;
        }
    }
}
