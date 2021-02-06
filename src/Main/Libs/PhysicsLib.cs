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

            };

            lua.L_NewLib(define);

            return 1;
        }
        
        public static int Raycast(ILuaState lua)
        {
            Vector3 origin = VectorLib.CheckVector(lua, 1);
            Vector3 direction = VectorLib.CheckVector(lua, 2);

            if (Physics.Raycast(origin, direction, out RaycastHit hit))
            {
                lua.NewTable(); //5, 1);

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

        public static int OverlapSphere(ILuaState lua)
        {
            Vector3 origin = VectorLib.CheckVector(lua, 1);
            float radius = (float) lua.L_CheckNumber(2);
            lua.NewTable();

            Collider[] colliders = Physics.OverlapSphere(origin, radius);
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
            
            return 1;
        }
    }
}
