using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class LinesLib
    {
        public static Material lineMaterial;

        public static void Init()
        {
            lineMaterial = new Material(Shader.Find("Unlit/Color"));
        }

        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("new_line_renderer", NewLineRenderer),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int NewLineRenderer(ILuaState lua)
        {
            GameObject luaLineRenderer = new GameObject("Lua Line Renderer");
            LuaLineRenderer llr = luaLineRenderer.AddComponent<LuaLineRenderer>();
            llr.lineRenderer.material = lineMaterial;
            llr.lineRenderer.SetWidth(0.5F, 0.5F);

            CSharpFunctionDelegate setPoints = (state) =>
            {
                List<Vector3> points = new List<Vector3>();

                for (int i = 1; i <= state.GetTop(); i++)
                    points.Add(VectorLib.CheckVector(lua, i));

                llr.lineRenderer.SetPositions(points.ToArray());
                return 1;
            };

            CSharpFunctionDelegate setWidth = (state) =>
            {
                float start = (float) state.L_CheckNumber(1);
                float end = (float) state.L_CheckNumber(2);
                llr.lineRenderer.SetWidth(start, end);
                return 1;
            };

            CSharpFunctionDelegate setColor = (state) =>
            {
                Vector4 start = VectorLib.CheckVector(state, 1);
                llr.lineRenderer.material.color = start;
                return 1;
            };

            lua.NewTable(); //2, 1);

            lua.PushCSharpFunction(setPoints);
            lua.SetField(-2, "set_points");

            lua.PushCSharpFunction(setWidth);
            lua.SetField(-2, "set_width");

            lua.PushCSharpFunction(setColor);
            lua.SetField(-2, "set_color");
            return 1;
        }
    }

    public class LuaLineRenderer : MonoBehaviour
    {
        public LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        private void FixedUpdate()
        {
            if (Machine.Active().SimulationMachine == null)
                Destroy(gameObject);
        }
    }
}
