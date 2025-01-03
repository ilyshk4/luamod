using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniLua;
using UnityEngine;

namespace LuaScripting.Libs
{
    public class EntitiesLib
    {
        public static Dictionary<LevelEntity, LevelEntityInfo> activeEntities = new Dictionary<LevelEntity, LevelEntityInfo>();
        public static void Init()
        {
            Events.OnSimulationToggle += (state) =>
            {
                if (state)
                {
                    try
                    {
                        Transform objs = null;
                        var a = GameObject.FindObjectsOfType<SetKinematicIfSim>();
                        foreach (var b in a)
                        {
                            if (b.name == "PHYSICS GOAL" && b.GetComponentInChildren<Rigidbody>())
                                objs = b.transform;
                        }

                        if (objs != null)
                            foreach (Transform obj in objs.transform)
                            {
                                var entity = obj.GetComponent<LevelEntity>();
                                var ai = obj.GetComponent<EntityAI>();
                                float maxHealth = 0;
                                if (ai != null)
                                    maxHealth = ai.health;
                                else
                                {
                                    foreach (var bf in entity.EntityBehaviour.breakForce)
                                        maxHealth += bf.ForceToBreak * 1000;
                                }
                                activeEntities.Add(entity, new LevelEntityInfo()
                                {
                                    entity = entity,
                                    ai = ai,
                                    maxHealth = maxHealth
                                });
                            }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[LuaScripting] Failed to get level entitites.");
                        Debug.LogError(e);
                    }

                }
                else
                {
                    activeEntities.Clear();
                }
            };
        }

        public static int OpenLib(ILuaState lua)
        {
            var define = new NameFuncPair[]
            {
                new NameFuncPair("count", Count),
                new NameFuncPair("get_all", GetAll),
                new NameFuncPair("get_all_by_name", GetAllByName),
                new NameFuncPair("get_all_by_id", GetAllByID),
                new NameFuncPair("get_all_by_category", GetAllByCategory),
                new NameFuncPair("get_all_in_sphere", GetAllInSphere),
                new NameFuncPair("get_nearest", GetNearest),
                new NameFuncPair("get_nearest_alive", GetNearestAlive),
            };

            lua.L_NewLib(define);

            return 1;
        }

        private static int Count(ILuaState lua)
        {
            lua.PushNumber(activeEntities.Count);
            return 1;
        }

        private static int GetAll(ILuaState lua)
        {
            lua.NewTable();

            List<LevelEntityInfo> ents = activeEntities.Values.ToList();

            for (int i = 0; i < ents.Count; i++)
            {
                PushLevelEntity(lua, ents[i].entity);
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }

        private static int GetAllByName(ILuaState lua)
        {
            string name = lua.L_CheckString(1);

            lua.NewTable();

            List<LevelEntityInfo> ents = activeEntities.Values.Where(le => le.entity.EntityBehaviour.prefab.name.ToLower() == name.ToLower()).ToList();

            for (int i = 0; i < ents.Count; i++)
            {
                PushLevelEntity(lua, ents[i].entity);
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }

        private static int GetAllByID(ILuaState lua)
        {
            int id = lua.L_CheckInteger(1);

            lua.NewTable();

            List<LevelEntityInfo> ents = activeEntities.Values.Where(le => le.entity.EntityBehaviour.prefab.ID == id).ToList();

            for (int i = 0; i < ents.Count; i++)
            {
                PushLevelEntity(lua, ents[i].entity);
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }

        private static int GetAllByCategory(ILuaState lua)
        {
            string name = lua.L_CheckString(1);

            lua.NewTable();

            List<LevelEntityInfo> ents = activeEntities.Values.Where(le => le.entity.EntityBehaviour.prefab.category.ToString().ToLower() == name.ToLower()).ToList();

            for (int i = 0; i < ents.Count; i++)
            {
                PushLevelEntity(lua, ents[i].entity);
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }

        private static int GetAllInSphere(ILuaState lua)
        {
            Vector3 pos = VectorLib.CheckVector(lua, 1);
            float radius = (float)lua.L_CheckNumber(2);

            lua.NewTable();

            List<LevelEntityInfo> ents = activeEntities.Values.Where(le => Vector3.Distance(le.entity.Position, pos) <= radius).ToList();

            for (int i = 0; i < ents.Count; i++)
            {
                PushLevelEntity(lua, ents[i].entity);
                lua.RawSetI(-2, i + 1);
            }

            return 1;
        }

        private static int GetNearest(ILuaState lua)
        {
            Vector3 pos = VectorLib.CheckVector(lua, 1);
            if (activeEntities.Count > 0)
            {
                var ents = activeEntities.Values.OrderBy(le => Vector3.Distance(le.entity.Position, pos)).ToArray();
                if (ents.Length > 0)
                    PushLevelEntity(lua, ents[0].entity);
                else return 0;
            }
            else return 0;
            return 1;
        }

        private static int GetNearestAlive(ILuaState lua)
        {
            Vector3 pos = VectorLib.CheckVector(lua, 1);
            if (activeEntities.Count > 0)
            {
                var ents = activeEntities.Values.Where(le => le.ai != null ? le.ai.health > 0 : !le.entity.IsDestroyed).OrderBy(le => Vector3.Distance(le.entity.Position, pos)).ToArray();
                if (ents.Length > 0)
                    PushLevelEntity(lua, ents[0].entity);
                else return 0;
            }
            else return 0;

            return 1;
        }

        public static void PushLevelEntity(ILuaState lua, LevelEntity levelEntity)
        {
            lua.NewTable();

            var ai = levelEntity.GetComponent<EntityAI>();
            var generic = levelEntity.GetComponent<AIGenericEntity>();

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.gameObject.transform.position));
            lua.SetField(-2, "position");

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.gameObject.transform.eulerAngles));
            lua.SetField(-2, "rotation");

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.Scale));
            lua.SetField(-2, "scale");

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.Velocity));
            lua.SetField(-2, "velocity");

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.IsDestroyed || (ai != null ? ai.health <= 0 : false)));
            lua.SetField(-2, "is_destroyed");

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.EntityBehaviour.prefab.ID));
            lua.SetField(-2, "id");

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.EntityBehaviour.prefab.name));
            lua.SetField(-2, "name");

            lua.PushCSharpFunction(Utils.CF(() => levelEntity.EntityBehaviour.prefab.category.ToString()));
            lua.SetField(-2, "category");

            lua.PushCSharpFunction(Utils.CF(() => generic != null ? (int)generic.Team : 0));
            lua.SetField(-2, "team");

            lua.PushCSharpFunction(Utils.CF(() => activeEntities[levelEntity].maxHealth));
            lua.SetField(-2, "max_health");

            lua.PushCSharpFunction(Utils.CF(() =>
            {
                if (ai != null)
                    return ai.health;
                else
                {
                    float health = 0;
                    foreach (var bf in levelEntity.EntityBehaviour.breakForce)
                        if (bf.BrokenInstance == null)
                            health += bf.ForceToBreak * 1000;
                    return health;
                }
            }));
            lua.SetField(-2, "health");
        }

        public struct LevelEntityInfo
        {
            public LevelEntity entity;
            public EntityAI ai;
            public float maxHealth;
        }
    }
}
