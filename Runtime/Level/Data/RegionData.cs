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
        public Dictionary<Tuple<int, int>, ChunkData> chunkDatas = new Dictionary<Tuple<int, int>, ChunkData>();
        public RegionData(int rid)
        {
            id = rid;
        }

        public string ToString()
        {
            string r = "Region: "+id;
            foreach (KeyValuePair<Tuple<int,int>,ChunkData> k in chunkDatas)
            {
                r += "\n" + k+ " "+k.Value.ToString();
            }
            return r;
        }
    }
}
