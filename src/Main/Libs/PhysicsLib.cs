using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class PhysicsLib
    {
        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("raycast", Raycast),
                new NameFuncPair("overlap_sphere", OverlapSphere),
                new NameFuncPair("overlap_box", OverlapBox),
                new NameFuncPair("overlap_capsule", OverlapCapsule),
                new NameFuncPair("check_sphere", CheckSphere),
                new NameFuncPair("check_box", CheckBox),
                new NameFuncPair("check_capsule", CheckCapsule),
                new NameFuncPair("sphere_cast", SphereCast),
                new NameFuncPair("box_cast", BoxCast),
                new NameFuncPair("capsule_cast", CapsuleCast),
                new NameFuncPair("line_cast", LineCast),
                new NameFuncPair("gravity", Utils.CF(() => Physics.gravity)),
            };
            lua.L_NewLib(define);
            return 1;
        }

        public static int Raycast(ILuaState lua)
        {
            Vector3 origin = VectorLib.CheckVector(lua, 1);
            Vector3 direction = VectorLib.CheckVector(lua, 2);
            float dist = lua.L_OptNumber(3, Mathf.Infinity);
            int layerMask = lua.L_OptInt(4, Physics.DefaultRaycastLayers);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, dist, layerMask))
            {
                lua.NewTable();

                BlockBehaviour hitBlock = hit.transform.GetComponent<BlockBehaviour>();

                lua.PushNumber(hit.distance);
                lua.SetField(-2, "distance");

                VectorLib.PushVector(lua, hit.point);
                lua.SetField(-2, "point");

                VectorLib.PushVector(lua, hit.normal);
                lua.SetField(-2, "normal");

                lua.PushBoolean(hitBlock != null);
                lua.SetField(-2, "is_block");

                lua.PushCSharpFunction((l) =>
                {
                    if (hitBlock != null)
                        MachineLib.PushBlockInfo(lua, hitBlock);
                    else
                        return 0;
                    return 1;
                });
                lua.SetField(-2, "get_block_info");

                return 1;
            }
            return 0;
        }

        public static int SphereCast(ILuaState lua)
        {
            Vector3 origin = VectorLib.CheckVector(lua, 1);
            Vector3 direction = VectorLib.CheckVector(lua, 2);
            float radius = (float)lua.L_CheckNumber(3);
            float maxDistance = (float)lua.L_CheckNumber(4);
            int layerMask = lua.L_OptInt(5, Physics.DefaultRaycastLayers);
            lua.PushBoolean(Physics.SphereCast(new Ray(origin, direction), radius, maxDistance, layerMask));
            return 1;
        }

        public static int BoxCast(ILuaState lua)
        {
            Vector3 center = VectorLib.CheckVector(lua, 1);
            Vector3 halfExtents = VectorLib.CheckVector(lua, 2);
            Vector3 direction = VectorLib.CheckVector(lua, 3);
            float maxDistance = (float)lua.L_CheckNumber(4);
            Quaternion quat = QuaternionLib.OptQuat(lua, 5, Quaternion.identity);
            int layerMask = lua.L_OptInt(6, Physics.DefaultRaycastLayers);
            lua.PushBoolean(Physics.BoxCast(center, halfExtents, direction, quat, maxDistance, layerMask));
            return 1;
        }

        public static int CapsuleCast(ILuaState lua)
        {
            Vector3 point0 = VectorLib.CheckVector(lua, 1);
            Vector3 point1 = VectorLib.CheckVector(lua, 2);
            float radius = (float)lua.L_CheckNumber(3);
            Vector3 dir = VectorLib.CheckVector(lua, 4);
            float maxDistance = (float)lua.L_CheckNumber(5);
            int layerMask = lua.L_OptInt(6, Physics.DefaultRaycastLayers);
            lua.PushBoolean(Physics.CapsuleCast(point0, point1, radius, dir, maxDistance, layerMask));
            return 1;
        }

        public static int LineCast(ILuaState lua)
        {
            Vector3 start = VectorLib.CheckVector(lua, 1);
            Vector3 end = VectorLib.CheckVector(lua, 2);
            int layerMask = lua.L_OptInt(3, Physics.DefaultRaycastLayers);
            lua.PushBoolean(Physics.Linecast(start, end, layerMask));
            return 1;
        }

        public static int CheckSphere(ILuaState lua)
        {
            Vector3 origin = VectorLib.CheckVector(lua, 1);
            float radius = (float)lua.L_CheckNumber(2);
            int layerMask = lua.L_OptInt(3, Physics.DefaultRaycastLayers);
            lua.PushBoolean(Physics.CheckSphere(origin, radius, layerMask));
            return 1;
        }

        public static int CheckBox(ILuaState lua)
        {
            Vector3 center = VectorLib.CheckVector(lua, 1);
            Vector3 halfExtents = VectorLib.CheckVector(lua, 2);
            Quaternion quat = QuaternionLib.OptQuat(lua, 3, Quaternion.identity);
            int layerMask = lua.L_OptInt(4, Physics.DefaultRaycastLayers);
            lua.PushBoolean(Physics.CheckBox(center, halfExtents, quat, layerMask));
            return 1;
        }

        public static int CheckCapsule(ILuaState lua)
        {
            Vector3 point0 = VectorLib.CheckVector(lua, 1);
            Vector3 point1 = VectorLib.CheckVector(lua, 2);
            float radius = (float)lua.L_CheckNumber(3);
            int layerMask = lua.L_OptInt(4, Physics.DefaultRaycastLayers);
            lua.PushBoolean(Physics.CheckCapsule(point0, point1, radius, layerMask));
            return 1;
        }

        public static int OverlapSphere(ILuaState lua)
        {
            Vector3 origin = VectorLib.CheckVector(lua, 1);
            float radius = (float)lua.L_CheckNumber(2);
            int layerMask = lua.L_OptInt(3, Physics.DefaultRaycastLayers);
            PushColliders(lua, Physics.OverlapSphere(origin, radius, layerMask));
            return 1;
        }

        public static int OverlapBox(ILuaState lua)
        {
            Vector3 center = VectorLib.CheckVector(lua, 1);
            Vector3 halfExtents = VectorLib.CheckVector(lua, 2);
            Quaternion quat = QuaternionLib.OptQuat(lua, 3, Quaternion.identity);
            int layerMask = lua.L_OptInt(4, Physics.DefaultRaycastLayers);
            PushColliders(lua, Physics.OverlapBox(center, halfExtents, quat, layerMask));
            return 1;
        }

        public static int OverlapCapsule(ILuaState lua)
        {
            Vector3 point0 = VectorLib.CheckVector(lua, 1);
            Vector3 point1 = VectorLib.CheckVector(lua, 2);
            float radius = (float)lua.L_CheckNumber(3);
            int layerMask = lua.L_OptInt(4, Physics.DefaultRaycastLayers);
            PushColliders(lua, Physics.OverlapCapsule(point0, point1, radius, layerMask));
            return 1;
        }

        public static void PushColliders(ILuaState lua, Collider[] colliders)
        {
            lua.NewTable();
            for (int i = 0; i < colliders.Length; i++)
            {
                lua.NewTable();

                BlockBehaviour block = colliders[i].gameObject.GetComponent<BlockBehaviour>();

                lua.PushBoolean(block != null);
                lua.SetField(-2, "is_block");

                lua.PushCSharpFunction((l) =>
                {
                    if (block != null)
                        MachineLib.PushBlockInfo(lua, block);
                    else
                        return 0;
                    return 1;
                });
                lua.SetField(-2, "get_block_info");

                lua.RawSetI(-2, i + 1);
            }
        }
    }
}
