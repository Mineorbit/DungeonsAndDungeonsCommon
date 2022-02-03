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

        public void LoadNearChunk(Vector3 position, bool immediate = false)
        {
            if (LevelManager.currentLevel != null)
                if (!loadedLocalChunks.Contains(ChunkManager.GetChunkGridPosition(position)))
                {
                    var chunkData = ChunkManager.ChunkToData(ChunkManager.GetChunk(position, true));
                    if (chunkData != null)
                    {
                        if (immediate)
                            Invoke(StreamChunkImmediateIntoCurrentLevelFrom, chunkData, true, true);
                        else
                            Invoke(StreamChunkIntoCurrentLevelFrom, chunkData, true , true);

                        loadedLocalChunks.Add(ChunkManager.GetChunkGridPosition(position));
                    }
                }
        }


        IEnumerator WaitRoutine(string cid,Action finishAction)
        {
            bool v = false;
            v = ChunkManager.ChunkLoaded(cid);
            while (!v)
            {
                v = ChunkManager.ChunkLoaded(cid);
                yield return new WaitForEndOfFrame();
            }
            mover.follow = true;
            MainCaller.Do(finishAction);  
        }
        
        public void WaitForChunkLoaded(Vector3 position, Action finishAction)
        {
            MainCaller.Do(() =>
            {
                mover.follow = false;
                transform.position = position;
                string chunkId = ChunkManager.GetChunkID(ChunkManager.GetChunkGridPosition(position));
                StartCoroutine(WaitRoutine(chunkId,finishAction));
            });
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
