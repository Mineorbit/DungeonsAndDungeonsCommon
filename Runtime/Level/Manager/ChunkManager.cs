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

        bool ready = false;


        public Dictionary<int, bool> regionLoaded = new Dictionary<int, bool>();
        public Dictionary<long, bool> chunkLoaded = new Dictionary<long, bool>();


        public enum LoadType { Disk, Net }

        void Start()
        {
            Setup();
        }


        static SaveManager regionDataLoader = new SaveManager(SaveManager.StorageType.BIN);

        public static void LoadRegion(int regionId, LoadType loadType = LoadType.Disk)
        {
            RegionData regionData = LoadRegionData(regionId,loadType: loadType);
            if (regionData == null) return;
            foreach (ChunkData chunkData in regionData.chunkDatas.Values)
            {
                ChunkManager.LoadChunk(chunkData);
            }
            instance.regionLoaded[regionId] = true;

        }

        public static RegionData LoadRegionData(Tuple<int, int> chunkPosition)
        {
            Tuple<int, int> regionPosition = GetRegionPosition(chunkPosition);
            int regionId;
            if(LevelDataManager.instance.levelData.regions.TryGetValue(regionPosition,out regionId))
            {
                return LoadRegionData(regionId);
            }
            return null;
        }

        static RegionData LoadRegionData(int regionId, LoadType loadType = LoadType.Disk)
        {
            if (instance.regionLoaded.ContainsKey(regionId) && instance.regionLoaded[regionId]) return null;
            if (loadType == LoadType.Disk)
            {
                string pathToLevel = LevelDataManager.instance.levelFolder.GetPath() + LevelManager.currentLevelMetaData.localLevelId;
                return regionDataLoader.Load<RegionData>(pathToLevel + "/" + regionId + ".bin");
            }
            else
            {
                // NOT YET NEEDED
                return null;
            }
        }



        public Dictionary<Tuple<int, int>, RegionData> FetchRegionData()
        {
            Dictionary<Tuple<int, int>, RegionData> regionData = new Dictionary<Tuple<int, int>, RegionData>();
            int rid = 0;
            foreach (KeyValuePair<Tuple<int,int>,Chunk> chunk in chunks)
            {
                Tuple<int, int> regionPosition = GetRegionPosition(chunk.Key);
                var positionInRegion = GetPositionInRegion(chunk.Key);

                ChunkData chunkData = ChunkData.FromChunk(chunk.Value);



                Debug.Log("Chunk at "+chunk.Key+" goes to Region "+regionPosition);


                RegionData r;

                if (regionData.TryGetValue(regionPosition, out r))
                {
                    r.chunkDatas.Add(new Tuple<int,int>(positionInRegion.a, positionInRegion.b),chunkData);
                }
                else
                {
                    r = new RegionData(rid);
                    rid++;
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

        static Tuple<int,int> GetRegionPosition(Tuple<int,int> chunkPosition)
        {
            int x =  (int)Mathf.Floor((float) chunkPosition.Item1 / (float)regionGranularity);
            int y =  (int)Mathf.Floor((float) chunkPosition.Item2 / (float)regionGranularity);
            Tuple<int, int> regionPosition = new Tuple<int, int>(x,y);
            return regionPosition;
        }

        public void Setup()
        {


            if (instance != null)
            {
                Destroy(this);
            }
            instance = this;


            if (!instance.ready)
            {

                chunkPrefab = Resources.Load("Chunk");
                chunks = new Dictionary<Tuple<int, int>, Chunk>();
                instance.ready = true;
            }
        }

        

        Chunk AddChunk(Tuple<int, int> gridPosition)
        {
            Setup();
            GameObject chunk = Instantiate(chunkPrefab) as GameObject;
            chunk.transform.parent = LevelManager.currentLevel.transform;
            chunk.transform.position = ChunkPositionFromGridPosition(gridPosition);
            var c = chunk.GetComponent<Chunk>();
            c.chunkId = GetChunkID(gridPosition);
            chunks.Add(gridPosition,c);
            chunkLoaded.Add(c.chunkId,true);
            return c;
        }

        
        long GetChunkID(Tuple<int,int> gridPosition)
        {
            byte[] x = BitConverter.GetBytes(gridPosition.Item1);
            byte[] y = BitConverter.GetBytes(gridPosition.Item2);
            byte[] l = new byte[8];
            Array.Copy(x,0,l,0,4);
            Array.Copy(y, 0, l, 4, 4);
            return BitConverter.ToInt64(l,0);
        }

        public static Vector3 ChunkPositionFromGridPosition(Tuple<int,int> gridPosition)
        {
            return new Vector3(gridPosition.Item1*chunkGranularity,0,gridPosition.Item2*chunkGranularity) - chunkGranularity / 2 * new Vector3(1, 0, 1);
        }

        public static Tuple<int,int> GetChunkGridPosition(Vector3 position)
        {
            return new Tuple<int, int>((int) Mathf.Round(position.x / chunkGranularity),(int) Mathf.Round(position.z / chunkGranularity));
        }

        Vector3 GetChunkPosition(Vector3 position)
        {
            return chunkGranularity * (new Vector3(Mathf.Round(position.x/chunkGranularity),0,Mathf.Round(position.z/chunkGranularity))) - chunkGranularity/2*new Vector3(1,0,1);
        }

        public static Chunk GetChunk(Vector3 position, bool createIfNotThere = true)
        {
            var chunkGridPosition = GetChunkGridPosition(position);
            Chunk result_chunk = null;
            if(instance.chunks != null)
            {
                if (instance.chunks.TryGetValue(chunkGridPosition,out result_chunk))
                {
                }
                else
                {
               if(createIfNotThere)
                    result_chunk = instance.AddChunk(chunkGridPosition);
                }
            }
            return result_chunk;
        }



        public static void LoadChunk(ChunkData chunkData)
        {
            List<LevelObjectInstanceData> levelObjectInstances = new List<LevelObjectInstanceData>();
            levelObjectInstances.AddRange(chunkData.levelObjects);
            foreach (LevelObjectInstanceData i in levelObjectInstances)
            {
                MainCaller.Do(() => {
                    Debug.Log("Adding "+i); LevelManager.currentLevel.Add(i); });
            }
            if(!instance.chunkLoaded.ContainsKey(chunkData.chunkId))
            { 
            instance.chunkLoaded.Add(chunkData.chunkId,true);
            }
            else
            {
            instance.chunkLoaded[chunkData.chunkId] = true;
            }
        }
    }
}
