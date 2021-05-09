using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTarget : NetworkLevelObject
    {
        List<Tuple<int, int>> loadedLocalChunks = new List<Tuple<int, int>>();

        void Start()
        {

        }


        void EnableChunkAt(Vector3 position)
        {
            if(!loadedLocalChunks.Contains(ChunkManager.GetChunkGridPosition(position)))
            {
                ChunkData chunkData = ChunkData.FromChunk(ChunkManager.GetChunk(position,createIfNotThere: true));
                if(chunkData != null)
                { 
                    Invoke(StreamChunkIntoCurrentLevelFrom,chunkData);
                }
                loadedLocalChunks.Add(ChunkManager.GetChunkGridPosition(position));
            }
        }

        void StreamChunkIntoCurrentLevelFrom(ChunkData chunkData)
        {
            Debug.Log("Streaming Chunk");
            ChunkManager.LoadChunk(chunkData);
        }

        void DisableChunk()
        {

        }

        //Overlap between load target zones is a problem
        void FixedUpdate()
        {
            EnableChunkAt(transform.position);
            EnableChunkAt(transform.position+transform.forward*32);
        }
    }
}
