using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class LevelObjectInstanceData
    {
        string type;
        float x;
        float y;
        float z;

        float q_x;
        float q_y;
        float q_z;
        float q_w;

        public static LevelObjectInstanceData FromInstance(LevelObject o)
        {
            LevelObjectInstanceData d = new LevelObjectInstanceData();
            Vector3 position = o.transform.position;
            Quaternion rotation = o.transform.rotation;
            d.x = position.x;
            d.y = position.y;
            d.z = position.z;

            d.q_x = rotation.x;
            d.q_y = rotation.y;
            d.q_z = rotation.z;
            d.q_w = rotation.w;
            d.type = o.type;
            return d;

        }
    }
}
