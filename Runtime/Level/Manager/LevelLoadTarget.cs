using System;
using System.Collections;
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


        public void Start()
        {
            LevelManager.levelClearEvent.AddListener(() => { loadedLocalChunks.Clear();});
        }

        //Overlap between load target zones is a problem
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test)
            {
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


        IEnumerator WaitRoutine(long cid,Action finishAction)
        {
            bool v = false;
            while (!v)
            {
                if(ChunkManager.chunkLoaded != null)
                {
                    Debug.Log("Chunk Dictionary there");
                    try
                    {
                        v = ChunkManager.chunkLoaded[cid];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e+ " writing default false");
                        v = false;
                    }
                    Debug.Log("Waiting for chunk load "+v);
                }
                else
                {
                    Debug.Log("Chunk Dictionary not there");
                    v = false;
                }
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("Wait for "+cid+" finished");
            mover.follow = true;
            MainCaller.Do(finishAction);  
        }
        
        public void WaitForChunkLoaded(Vector3 position, Action finishAction)
        {
            MainCaller.Do(() =>
            {
                mover.follow = false;
                transform.position = position;
                long chunkId = ChunkManager.GetChunkID(ChunkManager.GetChunkGridPosition(position));
                StartCoroutine(WaitRoutine(chunkId,finishAction));
            });
        }

        public void StreamChunkImmediateIntoCurrentLevelFrom(ChunkData chunkData)
        {
            Debug.Log("Streaming Chunk immediately " + chunkData.chunkId);
            ChunkManager.LoadChunk(chunkData, true);
        }

        public void StreamChunkIntoCurrentLevelFrom(ChunkData chunkData)
        {
            Debug.Log("Streaming Chunk " + chunkData.chunkId);
            ChunkManager.LoadChunk(chunkData);
        }


        private void DisableChunkAt(Vector3 position)
        {
        }
    }
}