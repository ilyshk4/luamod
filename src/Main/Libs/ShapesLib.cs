using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class ShapesLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("new_sphere", (s) => NewPrimitive(s, PrimitiveType.Sphere)),
                new NameFuncPair("new_capsule", (s) => NewPrimitive(s, PrimitiveType.Capsule)),
                new NameFuncPair("new_cylinder", (s) => NewPrimitive(s, PrimitiveType.Cylinder)),
                new NameFuncPair("new_cube", (s) => NewPrimitive(s, PrimitiveType.Cube)),
                new NameFuncPair("new_plane", (s) => NewPrimitive(s, PrimitiveType.Plane)),
                new NameFuncPair("new_quad", (s) => NewPrimitive(s, PrimitiveType.Quad)),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int NewPrimitive(ILuaState lua, PrimitiveType type)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            UnityEngine.Object.Destroy(primitive.GetComponent<Collider>());
            primitive.AddComponent<LuaSimulationTimeObject>();
            Vector3 position = VectorLib.CheckVector(lua, 1);
            Quaternion rotation = QuaternionLib.CheckQuat(lua, 2);
            Vector3 scale = VectorLib.CheckVector(lua, 3);
            Vector4 color = VectorLib.CheckVector(lua, 4);

            CSharpFunctionDelegate setPosition = (state) =>
            {
                primitive.transform.position = VectorLib.CheckVector(state, 1);
                return 0;
            };

            CSharpFunctionDelegate setRotation = (state) =>
            {
                primitive.transform.rotation = QuaternionLib.CheckQuat(state, 1);
                return 0;
            };

            CSharpFunctionDelegate setScale = (state) =>
            {
                primitive.transform.localScale = VectorLib.CheckVector(state, 1);
                return 0;
            };

            CSharpFunctionDelegate destroy = (state) =>
            {
                UnityEngine.Object.Destroy(primitive);
                return 0;
            };

            var material = primitive.GetComponent<Renderer>().material;

            CSharpFunctionDelegate setColor = (state) =>
            {
                Vector4 c = VectorLib.CheckVector(state, 1);
                material.color = new Color(c.x, c.y, c.z, c.w);
                return 0;
            };

            lua.NewTable();

            lua.PushCSharpFunction(setPosition);
            lua.SetField(-2, "set_position");

            lua.PushCSharpFunction(setRotation);
            lua.SetField(-2, "set_rotation");

            lua.PushCSharpFunction(setScale);
            lua.SetField(-2, "set_scale");

            lua.PushCSharpFunction(setColor);
            lua.SetField(-2, "set_color");

            lua.PushCSharpFunction(destroy);
            lua.SetField(-2, "destroy");

            primitive.transform.position = position;
            primitive.transform.rotation = rotation;
            primitive.transform.localScale = scale;

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            material.color = new Color(color.x, color.y, color.z, color.w);
            return 1;
        }
    }
}
