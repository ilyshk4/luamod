using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LuaScripting
{
    public class LuaPlayerMachine : MonoBehaviour
    {
        public Machine machine;
        public Dictionary<int, BlockInfo> blockInfos = new Dictionary<int, BlockInfo>();

        private float lastTime;

        private void Start()
        {
            if (machine.SimulationMachine)
                foreach (var block in machine.SimulationBlocks)
                {
                    BlockInfo info = new BlockInfo();
                    info.lastPosition = block.transform.position;
                    info.lastRotation = block.transform.rotation;

                    blockInfos.Add(block.BuildIndex, info);
                }
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.time - lastTime;
            if (machine.SimulationMachine)
                foreach (var block in machine.SimulationBlocks)
                {
                    if (blockInfos.ContainsKey(block.BuildIndex))
                    {
                        BlockInfo blockInfo = blockInfos[block.BuildIndex];

                        Vector3 calcVel = (block.transform.position - blockInfo.lastPosition) / deltaTime;
                        Vector3 calcAngVel = ToAngularVelocity(blockInfo.lastRotation, block.transform.rotation, deltaTime);

                        if (calcVel.magnitude == 0)
                            blockInfo.blockPositionFreezedCounter++;
                        else
                            blockInfo.blockPositionFreezedCounter = 0;

                        if (calcAngVel.magnitude == 0)
                            blockInfo.blockRotationFreezedCounter++;
                        else
                            blockInfo.blockRotationFreezedCounter = 0;

                        if (blockInfo.blockPositionFreezedCounter != 1)
                            blockInfo.velocity = calcVel;

                        if (blockInfo.blockRotationFreezedCounter != 1)
                            blockInfo.angularVelocity = calcAngVel;

                        blockInfo.lastPosition = block.transform.position;
                        blockInfo.lastRotation = block.transform.rotation;
                    }
                }
            lastTime = Time.time;
        }

        public class BlockInfo
        {
            public Vector3 lastPosition, velocity, angularVelocity;
            public Quaternion lastRotation;
            public uint blockPositionFreezedCounter;
            public uint blockRotationFreezedCounter;
        }

        static void ToEulerianAngle(Quaternion q, out float pitch, out float roll, out float yaw)
        {
            float ysqr = q.y * q.y;

            float t0 = +2.0f * (q.w * q.x + q.y * q.z);
            float t1 = +1.0f - 2.0f * (q.x * q.x + ysqr);
            roll = Mathf.Atan2(t0, t1);

            float t2 = +2.0f * (q.w * q.y - q.z * q.x);
            t2 = ((t2 > 1.0f) ? 1.0f : t2);
            t2 = ((t2 < -1.0f) ? -1.0f : t2);
            pitch = Mathf.Asin(t2);

            float t3 = +2.0f * (q.w * q.z + q.x * q.y);
            float t4 = +1.0f - 2.0f * (ysqr + q.z * q.z);
            yaw = Mathf.Atan2(t3, t4);
        }

        static Vector3 ToAngularVelocity(Quaternion start, Quaternion end, float delta_sec)
        {
            float p_s, r_s, y_s;
            ToEulerianAngle(start, out p_s, out r_s, out y_s);

            float p_e, r_e, y_e;
            ToEulerianAngle(end, out p_e, out r_e, out y_e);

            float p_rate = (p_e - p_s) / delta_sec;
            float r_rate = (r_e - r_s) / delta_sec;
            float y_rate = (y_e - y_s) / delta_sec;

            float wx = r_rate + 0 - y_rate * Mathf.Sin(p_e);
            float wy = 0 + p_rate * Mathf.Cos(r_e) + y_rate * Mathf.Sin(r_e) * Mathf.Cos(p_e);
            float wz = 0 - p_rate * Mathf.Sin(r_e) + y_rate * Mathf.Cos(r_e) * Mathf.Cos(p_e);

            return new Vector3(wx, wy, wz);
        }
    }
}
