using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ChunkManager : MonoBehaviour
    {
        public enum LoadType
        {
            Disk,
            Net
        }

        public static Object chunkPrefab;

        public static ChunkManager instance;

        public static float chunkGranularity = 32;

        public static int regionGranularity = 2;


        private static readonly SaveManager regionDataLoader = new SaveManager(SaveManager.StorageType.BIN);
        public Dictionary<long, bool> chunkLoaded = new Dictionary<long, bool>();

        public Dictionary<Tuple<int, int>, Chunk> chunks;
        public Dictionary<long, bool> finishedChunkLoaded = new Dictionary<long, bool>();

        private bool ready;


        public Dictionary<int, bool> regionLoaded = new Dictionary<int, bool>();

        private void Start()
        {
            Setup();
        }

        public static void LoadRegion(int regionId, LoadType loadType = LoadType.Disk)
        {
            var regionData = LoadRegionData(regionId, loadType);
            if (regionData == null) return;
            foreach (var chunkData in regionData.chunkDatas.Values) LoadChunk(chunkData);
            instance.regionLoaded[regionId] = true;
        }

        public static RegionData LoadRegionData(Tuple<int, int> chunkPosition)
        {
            var regionPosition = GetRegionPosition(chunkPosition);
            int regionId;
            if (LevelDataManager.instance.levelData.regions.TryGetValue(regionPosition, out regionId))
                return LoadRegionData(regionId);
            return null;
        }

        private static RegionData LoadRegionData(int regionId, LoadType loadType = LoadType.Disk)
        {
            if (instance.regionLoaded.ContainsKey(regionId) && instance.regionLoaded[regionId]) return null;
            if (loadType == LoadType.Disk)
            {
                var pathToLevel = LevelDataManager.instance.levelFolder.GetPath() +
                                  LevelManager.currentLevelMetaData.localLevelId;
                return regionDataLoader.Load<RegionData>(pathToLevel + "/" + regionId + ".bin");
            }

            // NOT YET NEEDED
            return null;
        }


        public Dictionary<Tuple<int, int>, RegionData> FetchRegionData()
        {
            var regionData = new Dictionary<Tuple<int, int>, RegionData>();
            var rid = 0;
            foreach (var chunk in chunks)
            {
                var regionPosition = GetRegionPosition(chunk.Key);
                var positionInRegion = GetPositionInRegion(chunk.Key);

                var chunkData = ChunkData.FromChunk(chunk.Value);


                Debug.Log("Chunk at " + chunk.Key + " goes to Region " + regionPosition);


                RegionData r;

                if (regionData.TryGetValue(regionPosition, out r))
                {
                    r.chunkDatas.Add(new Tuple<int, int>(positionInRegion.a, positionInRegion.b), chunkData);
                }
                else
                {
                    r = new RegionData(rid);
                    rid++;
                    r.chunkDatas.Add(new Tuple<int, int>(positionInRegion.a, positionInRegion.b), chunkData);
                    regionData.Add(regionPosition, r);
                }
            }

            return regionData;
        }

        private (int a, int b) GetPositionInRegion(Tuple<int, int> chunkPosition)
        {
            return (chunkPosition.Item1 % regionGranularity, chunkPosition.Item2 % regionGranularity);
        }

        private static Tuple<int, int> GetRegionPosition(Tuple<int, int> chunkPosition)
        {
            var x = (int) Mathf.Floor(chunkPosition.Item1 / (float) regionGranularity);
            var y = (int) Mathf.Floor(chunkPosition.Item2 / (float) regionGranularity);
            var regionPosition = new Tuple<int, int>(x, y);
            return regionPosition;
        }

        public void Setup()
        {
            if (instance != null) Destroy(this);
            instance = this;


            if (!instance.ready)
            {
                chunkPrefab = Resources.Load("Chunk");
                chunks = new Dictionary<Tuple<int, int>, Chunk>();
                instance.ready = true;
            }
        }


        private Chunk AddChunk(Tuple<int, int> gridPosition)
        {
            Setup();
            var chunk = Instantiate(chunkPrefab) as GameObject;
            chunk.transform.parent = LevelManager.currentLevel.transform;
            chunk.transform.position = ChunkPositionFromGridPosition(gridPosition);
            var c = chunk.GetComponent<Chunk>();
            c.chunkId = GetChunkID(gridPosition);
            chunks.Add(gridPosition, c);
            return c;
        }


        public static long GetChunkID(Tuple<int, int> gridPosition)
        {
            var x = BitConverter.GetBytes(gridPosition.Item1);
            var y = BitConverter.GetBytes(gridPosition.Item2);
            var l = new byte[8];
            Array.Copy(x, 0, l, 0, 4);
            Array.Copy(y, 0, l, 4, 4);
            return BitConverter.ToInt64(l, 0);
        }

        public static Vector3 ChunkPositionFromGridPosition(Tuple<int, int> gridPosition)
        {
            return new Vector3(gridPosition.Item1 * chunkGranularity, 0, gridPosition.Item2 * chunkGranularity) -
                   chunkGranularity / 2 * new Vector3(1, 0, 1);
        }

        public static Tuple<int, int> GetChunkGridPosition(Vector3 position)
        {
            return new Tuple<int, int>((int) Mathf.Round(position.x / chunkGranularity),
                (int) Mathf.Round(position.z / chunkGranularity));
        }

        private Vector3 GetChunkPosition(Vector3 position)
        {
            return chunkGranularity * new Vector3(Mathf.Round(position.x / chunkGranularity), 0,
                Mathf.Round(position.z / chunkGranularity)) - chunkGranularity / 2 * new Vector3(1, 0, 1);
        }

        public static Chunk GetChunk(Vector3 position, bool createIfNotThere = true)
        {
            var chunkGridPosition = GetChunkGridPosition(position);
            Chunk result_chunk = null;
            if (instance.chunks != null)
            {
                if (instance.chunks.TryGetValue(chunkGridPosition, out result_chunk))
                {
                }
                else
                {
                    if (createIfNotThere)
                        result_chunk = instance.AddChunk(chunkGridPosition);
                }
            }

            return result_chunk;
        }


        
        // Chunk Semantics
        // Not yet loaded: not in dictionary
        // Currently loading: in dictionary => false
        // Currently loaded: in dictionary => true
        public static void LoadChunk(ChunkData chunkData, bool immediate = true)
        {
            bool loaded;
            if (instance.chunkLoaded.TryGetValue(chunkData.chunkId, out loaded))
            {
                return;
            }
            else
            {
                instance.chunkLoaded.Add(chunkData.chunkId, false);
            }

            var levelObjectInstances = new List<LevelObjectInstanceData>();
            levelObjectInstances.AddRange(chunkData.levelObjects);
            var counter = levelObjectInstances.Count;
            Debug.Log(chunkData.chunkId + " adding " + counter + " Objects");
            foreach (var i in levelObjectInstances)
            {
                counter--;
                Action Complete = null;
                if (counter == 0)
                {
                    Complete = () =>
                    {
                    instance.finishedChunkLoaded[chunkData.chunkId] = true;
                    };
                }
                if (immediate)
                {
                    LevelManager.currentLevel.Add(i);
                    if (Complete != null)
                    {
                        Complete.Invoke();
                    }
                }
                else
                {
                    if (Complete != null)
                    {
                        MainCaller.Do(() =>
                        {
                            Debug.Log("Adding " + i);
                            LevelManager.currentLevel.Add(i);
                            Complete.Invoke();
                        }); 
                    }
                    else
                    {
                        MainCaller.Do(() =>
                        {
                            Debug.Log("Adding " + i);
                            LevelManager.currentLevel.Add(i);
                        });  
                    }
                    

                    
                
                }
            }
        }
    }
}