using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTarget : NetworkLevelObject
    {

        public enum LoadTargetMode { None, Near };

        public static LoadTargetMode loadTargetMode = LoadTargetMode.Near;

        List<Tuple<int, int>> loadedLocalChunks = new List<Tuple<int, int>>();

        void Start()
        {

        }


        void EnableChunkAt(Vector3 position)
        {
            if (loadTargetMode == LoadTargetMode.Near) LoadNearChunk(position);
        }

        public void LoadNearChunk(Vector3 position)
        {
            Debug.Log("Loading near "+position);
            if (LevelManager.currentLevel != null)
            {
                if (!loadedLocalChunks.Contains(ChunkManager.GetChunkGridPosition(position)))
                {
                    ChunkData chunkData = ChunkData.FromChunk(ChunkManager.GetChunk(position, createIfNotThere: true));
                    if (chunkData != null)
                    {
                        Invoke(StreamChunkIntoCurrentLevelFrom, chunkData);
                        loadedLocalChunks.Add(ChunkManager.GetChunkGridPosition(position));
                    }
                }
            }
        }

        public void StreamChunkIntoCurrentLevelFrom(ChunkData chunkData)
        {
            Debug.Log("Streaming Chunk "+chunkData.chunkId);
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
