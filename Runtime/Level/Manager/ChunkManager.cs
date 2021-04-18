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

        public static float chunkGranularity = 32;

        public static int regionGranularity = 2;

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

        

        public Dictionary<Tuple<int, int>, RegionData> FetchRegionData()
        {
            Dictionary<Tuple<int, int>, RegionData> regionData = new Dictionary<Tuple<int, int>, RegionData>();
            foreach (KeyValuePair<Tuple<int,int>,Chunk> chunk in chunks)
            {
                Tuple<int, int> regionPosition = GetRegionPosition(chunk.Key);
                var positionInRegion = GetPositionInRegion(chunk.Key);

                ChunkData chunkData = ChunkData.FromChunk(chunk.Value);



                int rid = 0;

                RegionData r;

                if (regionData.TryGetValue(regionPosition, out r))
                {
                    r.chunkDatas.Add(new Tuple<int,int>(positionInRegion.a, positionInRegion.b),chunkData);
                }
                else
                {
                    r = new RegionData(rid);
                    r.chunkDatas.Add(new Tuple<int, int>(positionInRegion.a, positionInRegion.b), chunkData);
                    regionData.Add(regionPosition,r);
                }
            }
            return regionData;
        }
        
        (int a,int b) GetPositionInRegion(Tuple<int,int> chunkPosition)
        {
            return (chunkPosition.Item1 % regionGranularity, chunkPosition.Item2 % regionGranularity);
        }

        Tuple<int,int> GetRegionPosition(Tuple<int,int> chunkPosition)
        {
            int x = regionGranularity*(int)Mathf.Floor(chunkPosition.Item1 / (float)regionGranularity);
            int y = regionGranularity * (int)Mathf.Floor(chunkPosition.Item2 / (float)regionGranularity);
            Tuple<int, int> regionPosition = new Tuple<int, int>(x,y);
            return regionPosition;
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
