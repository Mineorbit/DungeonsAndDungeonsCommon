using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ChunkManager : MonoBehaviour
    {
        public static UnityEngine.Object chunkPrefab;

        static Dictionary<Tuple<int, int>, Chunk> chunks;

        static float chunkGranularity = 32;

        static int count;


        void Start()
        {
            chunks = new Dictionary<Tuple<int, int>, Chunk>();
            chunkPrefab = Resources.Load("Chunk");
        }

        public void Load()
        {

        }

        public void Save()
        {

        }

        static void AddChunk(Tuple<int, int> gridPosition)
        {
            count++;
            GameObject chunk = Instantiate(chunkPrefab) as GameObject;
            chunk.name = "Chunk " + count;
            chunk.transform.parent = LevelManager.currentLevel.transform;
            chunk.transform.position = ChunkPositionFromGridPosition(gridPosition);
            chunks.Add(gridPosition,chunk.GetComponent<Chunk>());
        }

        static Vector3 ChunkPositionFromGridPosition(Tuple<int,int> gridPosition)
        {
            return new Vector3(gridPosition.Item1*chunkGranularity,0,gridPosition.Item2*chunkGranularity) - chunkGranularity / 2 * new Vector3(1, 0, 1);
        }

        static Tuple<int,int> GetChunkGridPosition(Vector3 position)
        {
            return new Tuple<int, int>((int) Mathf.Round(position.x / chunkGranularity),(int) Mathf.Round(position.z / chunkGranularity));
        }

        static Vector3 GetChunkPosition(Vector3 position)
        {
            return chunkGranularity * (new Vector3(Mathf.Round(position.x/chunkGranularity),0,Mathf.Round(position.z/chunkGranularity))) - chunkGranularity/2*new Vector3(1,0,1);
        }

        public static void GetChunk(Vector3 position)
        {
            AddChunk(GetChunkGridPosition(position));
        }
    }
}
