using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetLevel;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTarget : Entity
    {
        public enum LoadTargetMode
        {
            None,
            Near
        }

        public static LoadTargetMode loadTargetMode = LoadTargetMode.None;

        public LevelLoadTargetMover mover;

        private readonly List<Tuple<int, int, int>> loadedLocalChunks = new List<Tuple<int, int, int>>();


        public override void Start()
        {
            base.Start();
            LevelManager.levelClearEvent.AddListener(() => { loadedLocalChunks.Clear();});
        }

        private int radius = 2;
        //Overlap between load target zones is a problem
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test)
            {
                for(int i = - radius;i<=radius;i++)
                    for(int j = - radius; j<=radius;j++)
                        for (int k = -radius; k <= radius; k++)
                        {
                            EnableChunkAt(transform.position+ChunkManager.chunkGranularity*(new Vector3(i,j,k)));
                        }
                
            }
        }

        private void EnableChunkAt(Vector3 position)
        {
            if (loadTargetMode == LoadTargetMode.Near) LoadNearChunk(position);
        }

        public void LoadNearChunk(Vector3 position)
        {
            if (LevelManager.currentLevel != null)
                if (!loadedLocalChunks.Contains(ChunkManager.GetChunkGridPosition(position)))
                {
                    Chunk chunk = ChunkManager.GetChunk(position, false);
                    if (chunk != null)
                    {
                        
                        var chunkData = ChunkManager.ChunkToData(chunk);
                        if (chunkData != null)
                        {
                            Invoke(StreamChunkIntoCurrentLevelFrom, chunkData, true , true);
                            loadedLocalChunks.Add(ChunkManager.GetChunkGridPosition(position));
                        }
                    
                    }
                }
        }


        

        public void StreamChunkImmediateIntoCurrentLevelFrom(ChunkData chunkData)
        {
            GameConsole.Log("Streaming Chunk immediately " + chunkData.ChunkId);
            ChunkManager.LoadChunk(chunkData, true);
        }

        public void StreamChunkIntoCurrentLevelFrom(ChunkData chunkData)
        {
            Debug.Log("Streaming Chunk " + chunkData.ChunkId);
            ChunkManager.LoadChunk(chunkData);
        }


        private void DisableChunkAt(Vector3 position)
        {
        }
    }
}
