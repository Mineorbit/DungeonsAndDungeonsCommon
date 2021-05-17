using System;
using System.Collections.Generic;

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
            var r = "Region: " + id;
            foreach (var k in chunkDatas) r += "\n" + k + " " + k.Value.ToString();
            return r;
        }
    }
}