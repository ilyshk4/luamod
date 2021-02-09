using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class GUILib
    {
        public static NameFuncPair[] define;

        public static void Init()
        {
            define = new NameFuncPair[]
            {
                new NameFuncPair("Toggle", Toggle),
                new NameFuncPair("Label", Label),
                new NameFuncPair("Button", Button),
                new NameFuncPair("BeginGroup", BeginGroup),
                new NameFuncPair("BeginScrollView", BeginScrollView),
                new NameFuncPair("Box", Box),
                new NameFuncPair("BringWindowToBack", BringWindowToBack),
                new NameFuncPair("BringWindowToFront", BringWindowToFront),
                new NameFuncPair("DragWindow", DragWindow),
                new NameFuncPair("EndGroup", EndGroup),
                new NameFuncPair("EndScrollView", EndScrollView),
                new NameFuncPair("FocusControl", FocusControl),
                new NameFuncPair("FocusWindow", FocusWindow),
                new NameFuncPair("GetNameOfFocusedControl", GetNameOfFocusedControl),
                new NameFuncPair("HorizontalScrollbar", HorizontalScrollbar),
                new NameFuncPair("HorizontalSlider", HorizontalSlider),
                new NameFuncPair("ModalWindow", ModalWindow),
                new NameFuncPair("PasswordField", PasswordField),
                new NameFuncPair("RepeatButton", RepeatButton),
                new NameFuncPair("ScrollTo", ScrollTo),
                new NameFuncPair("SelectionGrid", SelectionGrid),
                new NameFuncPair("SetNextControlName", SetNextControlName),
                new NameFuncPair("TextArea", TextArea),
                new NameFuncPair("TextField", TextField),
                new NameFuncPair("Toolbar", Toolbar),
                new NameFuncPair("UnfocusWindow", UnfocusWindow),
                new NameFuncPair("VerticalScrollbar", VerticalScrollbar),
                new NameFuncPair("VerticalSlider", VerticalSlider),
                new NameFuncPair("Window", Window),
                new NameFuncPair("WorldToScreenPoint", WorldToScreenPoint),
            };

            define = Utils.MakePretty(define);
        }

        public static int OpenLib(ILuaState lua)
        {
            lua.L_NewLib(define);

            return 1;
        }

        public static int Label(ILuaState lua)
        {
            GUI.Label(RectLib.CheckRect(lua, 1), lua.L_CheckString(2));
            return 0;
        }

        public static int WorldToScreenPoint(ILuaState lua)
        {
            Vector2 pos = LuaScripting.Instance.mainCamera.WorldToScreenPoint(VectorLib.CheckVector(lua, 1));
            pos.y = Screen.height - pos.y;
            VectorLib.PushVector(lua, pos);
            return 1;
        }

        public static int Toggle(ILuaState lua)
        {
            lua.PushBoolean(GUI.Toggle(RectLib.CheckRect(lua, 1), lua.ToBoolean(2), lua.L_CheckString(3)));
            return 1;
        }

        public static int Button(ILuaState lua)
        {
            lua.PushBoolean(GUI.Button(RectLib.CheckRect(lua, 1), lua.L_CheckString(2)));
            return 1;
        }

        public static int BeginGroup(ILuaState lua)
        {
            GUI.BeginGroup(RectLib.CheckRect(lua, 1));
            return 0;
        }

        public static int BeginScrollView(ILuaState lua)
        {
            GUI.BeginScrollView(RectLib.CheckRect(lua, 1), VectorLib.CheckVector(lua, 2), RectLib.CheckRect(lua, 3));
            return 0;
        }

        public static int Box(ILuaState lua)
        {
            GUI.Box(RectLib.CheckRect(lua, 1), lua.L_CheckString(2));
            return 0;
        }

        public static int BringWindowToBack(ILuaState lua)
        {
            GUI.BringWindowToBack(lua.L_CheckInteger(1));
            return 0;
        }

        public static int BringWindowToFront(ILuaState lua)
        {
            GUI.BringWindowToFront(lua.L_CheckInteger(1));
            return 0;
        }
        public static int DragWindow(ILuaState lua)
        {
            GUI.DragWindow();
            return 0;
        }

        public static int EndGroup(ILuaState lua)
        {
            GUI.EndGroup();
            return 0;
        }

        public static int EndScrollView(ILuaState lua)
        {
            GUI.EndScrollView();
            return 0;
        }

        public static int FocusControl(ILuaState lua)
        {
            GUI.FocusControl(lua.L_CheckString(1));
            return 0;
        }

        public static int FocusWindow(ILuaState lua)
        {
            GUI.FocusWindow(lua.L_CheckInteger(1));
            return 0;
        }

        public static int GetNameOfFocusedControl(ILuaState lua)
        {
            lua.PushString(GUI.GetNameOfFocusedControl());
            return 0;
        }

        public static int HorizontalScrollbar(ILuaState lua)
        {
            lua.PushNumber(GUI.HorizontalScrollbar(Libs.RectLib.CheckRect(lua, 1), (float) lua.L_CheckNumber(2), (float) lua.L_CheckNumber(3), (float) lua.L_CheckNumber(4), (float) lua.L_CheckNumber(5)));
            return 1;
        }

        public static int HorizontalSlider(ILuaState lua)
        {
            lua.PushNumber(GUI.HorizontalSlider(Libs.RectLib.CheckRect(lua, 1), (float) lua.L_CheckNumber(2), (float) lua.L_CheckNumber(3), (float) lua.L_CheckNumber(4)));
            return 1;
        }

        public static int ModalWindow(ILuaState lua)
        {
            int id = lua.L_CheckInteger(1);
            Rect rect = RectLib.CheckRect(lua, 2);
            string title = lua.L_CheckString(3);
            int fRef = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);

            Rect r = GUI.ModalWindow(id, rect, (i) =>
            {

                lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, fRef);
                lua.PushInteger(i);
                if (lua.PCall(1, 0, 0) != ThreadStatus.LUA_OK)
                    Debug.LogError(lua.ToString(-1));
            }, title);

            RectLib.PushRect(lua, r);

            return 1;
        }

        public static int PasswordField(ILuaState lua)
        {
            lua.PushString(GUI.PasswordField(RectLib.CheckRect(lua, 1), lua.L_CheckString(2), lua.L_CheckString(3)[0]));
            return 1;
        }

        public static int RepeatButton(ILuaState lua)
        {
            lua.PushBoolean(GUI.RepeatButton(RectLib.CheckRect(lua, 1), lua.L_CheckString(2)));
            return 1;
        }

        public static int ScrollTo(ILuaState lua)
        {
            GUI.ScrollTo(RectLib.CheckRect(lua, 1));
            return 0;
        }

        public static int SelectionGrid(ILuaState lua)
        {
            lua.PushInteger(GUI.SelectionGrid(RectLib.CheckRect(lua, 1), lua.L_CheckInteger(2), lua.CheckStringArray(3), lua.L_CheckInteger(4)));
            return 1;
        }

        public static int SetNextControlName(ILuaState lua)
        {
            GUI.SetNextControlName(lua.L_CheckString(1));
            return 0;
        }

        public static int TextArea(ILuaState lua)
        {
            lua.PushString(GUI.TextArea(RectLib.CheckRect(lua, 1), lua.L_CheckString(2)));
            return 1;
        }

        public static int TextField(ILuaState lua)
        {
            lua.PushString(GUI.TextField(RectLib.CheckRect(lua, 1), lua.L_CheckString(2)));
            return 1;
        }

        public static int Toolbar(ILuaState lua)
        {
            lua.PushInteger(GUI.Toolbar(RectLib.CheckRect(lua, 1), lua.L_CheckInteger(2), lua.CheckStringArray(3)));
            return 1;
        }

        public static int UnfocusWindow(ILuaState lua)
        {
            GUI.UnfocusWindow();
            return 0;
        }

        public static int VerticalScrollbar(ILuaState lua)
        {
            lua.PushNumber(GUI.VerticalScrollbar(Libs.RectLib.CheckRect(lua, 1), (float) lua.L_CheckNumber(2), (float) lua.L_CheckNumber(3), (float) lua.L_CheckNumber(4), (float) lua.L_CheckNumber(5)));
            return 1;
        }
        public static int VerticalSlider(ILuaState lua)
        {
            lua.PushNumber(GUI.VerticalSlider(Libs.RectLib.CheckRect(lua, 1), (float) lua.L_CheckNumber(2), (float) lua.L_CheckNumber(3), (float) lua.L_CheckNumber(4)));
            return 1;
        }

        public static int Window(ILuaState lua)
        {
            int id = lua.L_CheckInteger(1);
            Rect rect = RectLib.CheckRect(lua, 2);
            string title = lua.L_CheckString(3);
            int fRef = lua.L_Ref(LuaDef.LUA_REGISTRYINDEX);

            Rect r = GUI.Window(id, rect, (i) =>
            {
                lua.RawGetI(LuaDef.LUA_REGISTRYINDEX, fRef);
                lua.PushInteger(i);
                if (lua.PCall(1, 0, 0) != ThreadStatus.LUA_OK)
                    Debug.LogError(lua.ToString(-1));
            }, title);

            RectLib.PushRect(lua, r);

            return 1;
        }
    }
}
