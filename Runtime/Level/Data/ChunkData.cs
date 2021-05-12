using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class ChunkData
    {
        public long chunkId;
        public List<LevelObjectInstanceData> levelObjects = new List<LevelObjectInstanceData>();

        public static ChunkData FromChunk(Chunk c)
        {
            ChunkData chunkData = new ChunkData();
            Debug.Log(c);
            chunkData.chunkId = c.chunkId;
            List<LevelObject> levelObjectsOfChunk = c.GetComponentsInChildren<LevelObject>(includeInactive: true).ToList();
            List<LevelObjectInstanceData> levelObjectInstanceData = levelObjectsOfChunk.Select((x)=> { return (LevelObjectInstanceData) LevelObjectInstanceData.FromInstance(x as dynamic); }).ToList();
            chunkData.levelObjects = levelObjectInstanceData;

            Debug.Log(chunkData.ToString());
            return chunkData;
        }

        public static NetLevel.ChunkData ToNetData(ChunkData inData)
        {

            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memoryStream, inData);
            memoryStream.Position = 0;
            Debug.Log("Length to send: " + memoryStream.Length);
            NetLevel.ChunkData chunkData = new NetLevel.ChunkData
            {
                ChunkId = inData.chunkId,
                Data = Google.Protobuf.ByteString.FromStream(memoryStream)
            };
            return chunkData;
        }

        public static ChunkData FromNetData(NetLevel.ChunkData netChunkData)
        {
            ChunkData outData = new ChunkData();
            Debug.Log("DATEN: " + netChunkData.Data.ToByteArray().Length);
            MemoryStream memoryStream = new MemoryStream(netChunkData.Data.ToByteArray());
            memoryStream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            return (ChunkData) bf.Deserialize(memoryStream);
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
