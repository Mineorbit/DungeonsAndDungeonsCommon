using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class RegionData
    {
        public int id;
        public ChunkData[,] chunkDatas = new ChunkData[ChunkManager.regionGranularity,ChunkManager.regionGranularity];
        public RegionData(int rid)
        {
            id = rid;
        }
    }
}
