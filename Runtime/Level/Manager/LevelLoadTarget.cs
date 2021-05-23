using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTarget : NetworkLevelObject
    {
        public enum LoadTargetMode
        {
            None,
            Near
        }

        public static LoadTargetMode loadTargetMode = LoadTargetMode.None;

        public LevelLoadTargetMover mover;

        private readonly List<Tuple<int, int>> loadedLocalChunks = new List<Tuple<int, int>>();


        private void Start()
        {
        }


       

        //Overlap between load target zones is a problem
        public override void FixedUpdate()
        {
            if(Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test)
            {
            base.FixedUpdate();
            EnableChunkAt(transform.position);
            EnableChunkAt(transform.position + transform.forward * 32);
            EnableChunkAt(transform.position - transform.forward * 32);
            EnableChunkAt(transform.position + transform.right * 32);
            EnableChunkAt(transform.position - transform.right * 32);
            
            }
        }

        private void EnableChunkAt(Vector3 position)
        {
            if (loadTargetMode == LoadTargetMode.Near) LoadNearChunk(position);
        }

        public void LoadNearChunk(Vector3 position, bool immediate = false)
        {
            if (LevelManager.currentLevel != null)
                if (!loadedLocalChunks.Contains(ChunkManager.GetChunkGridPosition(position)))
                {
                    var chunkData = ChunkData.FromChunk(ChunkManager.GetChunk(position, true));
                    if (chunkData != null)
                    {
                        if (immediate)
                            Invoke(StreamChunkImmediateIntoCurrentLevelFrom, chunkData);
                        else
                            Invoke(StreamChunkIntoCurrentLevelFrom, chunkData);

                        loadedLocalChunks.Add(ChunkManager.GetChunkGridPosition(position));
                    }
                }
        }


        public void WaitForChunkLoaded(Vector3 position, Action finishAction)
        {
            MainCaller.Do(() =>
            {
                if(LevelManager.currentLevel != null)
                {
                var store = mover.target;
                mover.target = null;
                transform.position = position;
                var id = ChunkManager.GetChunkID(ChunkManager.GetChunkGridPosition(position));
                LoadNearChunk(position, true);
                Task.Run(async () =>
                {
                    while (LevelManager.currentLevel == null || !ChunkManager.instance.chunkLoaded.ContainsKey(id) ||
                           !ChunkManager.instance.chunkLoaded[id])
                    {
                        await Task.Delay(100);
                    }
                });

                mover.target = store;
                }
                MainCaller.Do(finishAction);
            });
        }

        public void StreamChunkImmediateIntoCurrentLevelFrom(ChunkData chunkData)
        {
            Debug.Log("Streaming Chunk " + chunkData.chunkId);
            ChunkManager.LoadChunk(chunkData, true);
        }

        public void StreamChunkIntoCurrentLevelFrom(ChunkData chunkData)
        {
            Debug.Log("Streaming Chunk " + chunkData.chunkId);
            ChunkManager.LoadChunk(chunkData);
        }


        private void DisableChunk()
        {
        }
    }
}