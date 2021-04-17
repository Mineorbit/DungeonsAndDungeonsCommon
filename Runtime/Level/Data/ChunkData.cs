using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class ChunkData
    {
        List<LevelObjectInstanceData> levelObjects = new List<LevelObjectInstanceData>();

        public static ChunkData FromChunk(Chunk c)
        {
            ChunkData chunkData = new ChunkData();
            foreach(Transform t in c.transform)
            {
                chunkData.levelObjects.Add(LevelObjectInstanceData.FromInstance(t.GetComponent<LevelObject>()));
            }
            return chunkData;
        }
    }
}
