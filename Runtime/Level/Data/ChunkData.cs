using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class ChunkData
    {
        public int chunkId;
        public List<LevelObjectInstanceData> levelObjects = new List<LevelObjectInstanceData>();

        public static ChunkData FromChunk(Chunk c)
        {
            ChunkData chunkData = new ChunkData();
            chunkData.chunkId = c.chunkId;
            List<LevelObject> levelObjectsOfChunk = c.GetComponentsInChildren<LevelObject>(includeInactive: true).ToList();
            List<LevelObjectInstanceData> levelObjectInstanceData = levelObjectsOfChunk.Select((x)=> { return (LevelObjectInstanceData) LevelObjectInstanceData.FromInstance(x as dynamic); }).ToList();
            chunkData.levelObjects = levelObjectInstanceData;

            Debug.Log(chunkData.ToString());
            return chunkData;
        }

        public string ToString()
        {
            string r = "Chunk: "+chunkId;
            foreach(LevelObjectInstanceData d in levelObjects)
            {
                r += "\n"+d.ToString();
            }
            return r;
        }
    }
}
