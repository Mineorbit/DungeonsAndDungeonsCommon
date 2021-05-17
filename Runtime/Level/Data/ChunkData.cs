using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class ChunkData
    {
        public long chunkId;
        public List<LevelObjectInstanceData> levelObjects = new List<LevelObjectInstanceData>();

        public static ChunkData FromChunk(Chunk c)
        {
            var chunkData = new ChunkData();
            chunkData.chunkId = c.chunkId;
            var levelObjectsOfChunk = c.GetComponentsInChildren<LevelObject>(true).ToList();
            var levelObjectInstanceData = levelObjectsOfChunk.Select(x =>
            {
                return (LevelObjectInstanceData) LevelObjectInstanceData.FromInstance(x as dynamic);
            }).ToList();
            chunkData.levelObjects = levelObjectInstanceData;

            return chunkData;
        }

        public static NetLevel.ChunkData ToNetData(ChunkData inData)
        {
            var memoryStream = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(memoryStream, inData);
            memoryStream.Position = 0;
            Debug.Log("Length to send: " + memoryStream.Length);
            var chunkData = new NetLevel.ChunkData
            {
                ChunkId = inData.chunkId,
                Data = ByteString.FromStream(memoryStream)
            };
            return chunkData;
        }

        public static ChunkData FromNetData(NetLevel.ChunkData netChunkData)
        {
            var outData = new ChunkData();
            Debug.Log("DATEN: " + netChunkData.Data.ToByteArray().Length);
            var memoryStream = new MemoryStream(netChunkData.Data.ToByteArray());
            memoryStream.Position = 0;
            var bf = new BinaryFormatter();
            return (ChunkData) bf.Deserialize(memoryStream);
        }

        public string ToString()
        {
            var r = "Chunk: " + chunkId;
            foreach (var d in levelObjects) r += "\n" + d;
            return r;
        }
    }
}