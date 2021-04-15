using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ChunkManager : MonoBehaviour
    {
        public static UnityEngine.Object chunkPrefab;

        public static ChunkManager instance;

        public Dictionary<Tuple<int, int>, Chunk> chunks;

        static float chunkGranularity = 32;

        public int count;

        bool ready = false;
        
        void Start()
        {
            if(instance != null)
            {
                Destroy(this);
            } 
            instance = this;
            Setup();
        }

        void Setup()
        {
            if(!ready)
            {
                chunkPrefab = Resources.Load("Chunk");
                chunks = new Dictionary<Tuple<int, int>, Chunk>();
                ready = true;
            }
        }

        public void Load()
        {

        }

        public void Save()
        {

        }

        Chunk AddChunk(Tuple<int, int> gridPosition)
        {
            Setup();
            count++;
            GameObject chunk = Instantiate(chunkPrefab) as GameObject;
            chunk.transform.parent = LevelManager.currentLevel.transform;
            chunk.transform.position = ChunkPositionFromGridPosition(gridPosition);
            var c = chunk.GetComponent<Chunk>();
            c.chunkId = count;
            chunks.Add(gridPosition,c);
            return c;
        }

        

        Vector3 ChunkPositionFromGridPosition(Tuple<int,int> gridPosition)
        {
            return new Vector3(gridPosition.Item1*chunkGranularity,0,gridPosition.Item2*chunkGranularity) - chunkGranularity / 2 * new Vector3(1, 0, 1);
        }

        Tuple<int,int> GetChunkGridPosition(Vector3 position)
        {
            return new Tuple<int, int>((int) Mathf.Round(position.x / chunkGranularity),(int) Mathf.Round(position.z / chunkGranularity));
        }

        Vector3 GetChunkPosition(Vector3 position)
        {
            return chunkGranularity * (new Vector3(Mathf.Round(position.x/chunkGranularity),0,Mathf.Round(position.z/chunkGranularity))) - chunkGranularity/2*new Vector3(1,0,1);
        }

        public static Chunk GetChunk(Vector3 position)
        {
            var chunkGridPosition = instance.GetChunkGridPosition(position);
            Chunk result_chunk = null;
            if(instance.chunks != null)
            { 
            if (instance.chunks.TryGetValue(chunkGridPosition,out result_chunk))
            {
                // chunk was found here eventually something
            }else
            {
                result_chunk = instance.AddChunk(chunkGridPosition);
            }
            }
            return result_chunk;
        }
    }
}
