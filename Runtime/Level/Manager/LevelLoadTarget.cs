using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTarget : NetworkLevelObject
    {

        public enum LoadTargetMode { None, Near };

        public static LoadTargetMode loadTargetMode = LoadTargetMode.None;

        List<Tuple<int, int>> loadedLocalChunks = new List<Tuple<int, int>>();

        public Transform target;



        void Start()
        {

        }


        public void Update()
        {
            if(target != null)
            {
                target.position = transform.position;
            }
        }

        void EnableChunkAt(Vector3 position)
        {
            if (loadTargetMode == LoadTargetMode.Near)
            {
                LoadNearChunk(position);
            }
        }

        public void LoadNearChunk(Vector3 position)
        {
            if (LevelManager.currentLevel != null)
            {
                if (!loadedLocalChunks.Contains(ChunkManager.GetChunkGridPosition(position)))
                {
                    ChunkData chunkData = ChunkData.FromChunk(ChunkManager.GetChunk(position, createIfNotThere: true));
                    if (chunkData != null)
                    {
                        Invoke(StreamChunkIntoCurrentLevelFrom,chunkData);
                        loadedLocalChunks.Add(ChunkManager.GetChunkGridPosition(position));
                    }
                }

            }
        }


        public void WaitForChunkLoaded(Vector3 position)
        {
            MainCaller.Do(() => {
            Debug.Log("Started WAITING");
            Transform store = target;
            target = null;
            transform.position = position;
            long id = ChunkManager.GetChunkID(ChunkManager.GetChunkGridPosition(position));

            Debug.Log("ID: "+id);

            Debug.Log("Started WAITING");
                Task.Run(async () =>
                {
                    while (LevelManager.currentLevel == null || !ChunkManager.instance.chunkLoaded.ContainsKey(id) || !ChunkManager.instance.chunkLoaded[id])
                    {
                        Debug.Log("WAITING" + LevelManager.currentLevel == null + " " + !ChunkManager.instance.chunkLoaded.ContainsKey(id) + " " + !ChunkManager.instance.chunkLoaded[id]);
                        await Task.Delay(100);
                    }
                });

            Debug.Log("Finished Loading");
            target = store;
            });
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
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            EnableChunkAt(transform.position);
            EnableChunkAt(transform.position + transform.forward * 32);
            EnableChunkAt(transform.position - transform.forward * 32);
            EnableChunkAt(transform.position + transform.right * 32);
            EnableChunkAt(transform.position - transform.right * 32);
        }
    }
}
