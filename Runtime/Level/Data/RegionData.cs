using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class RegionData : ScriptableObject
    {
        ChunkData[,] chunkDatas = new ChunkData[4,4];
    }
}
