using System;
using System.Collections.Generic;
using System.Linq;
using NetLevel;
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

        private static readonly ProtoSaveManager regionDataLoader = new ProtoSaveManager();

        public Dictionary<Tuple<int, int>, Chunk> chunks;
        
        public static Dictionary<long, bool> chunkLoaded = new Dictionary<long, bool>();

        private bool ready;

        public Dictionary<int, bool> regionLoaded = new Dictionary<int, bool>();

        private void Start()
        {
            Setup();
        }

        
        public void Setup()
        {
            if (instance != null) Destroy(this);
            instance = this;

            Reset();
        }

        public void Reset()
        {
            if(chunkLoaded == null)
            {
                chunkLoaded = new Dictionary<long, bool>();
            }
            chunkLoaded.Clear();
                chunkPrefab = Resources.Load("Chunk");
                chunks = new Dictionary<Tuple<int, int>, Chunk>();
                
        }
        
        public static void LoadRegion(int regionId, LoadType loadType = LoadType.Disk)
        {
            var regionData = LoadRegionData(regionId, loadType);
            if (regionData == null) return;
            foreach (var chunkLocationPair in regionData.ChunkDatas) LoadChunk(chunkLocationPair.Chunk);
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
                var pathToLevel = LevelDataManager.GetLevelPath(LevelManager.currentLevelMetaData);
                return ProtoSaveManager.Load<RegionData>(pathToLevel + "/" + regionId + ".bin");
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

                ChunkData chunkData = ChunkManager.ChunkToData(chunk.Value);


                Debug.Log("Chunk at " + chunk.Key + " goes to Region " + regionPosition);


                RegionData r;

                if (regionData.TryGetValue(regionPosition, out r))
                {
                    CoordinateChunkPair coordinateChunkPair = new CoordinateChunkPair();
                    coordinateChunkPair.X = positionInRegion.a;
                    coordinateChunkPair.Y = positionInRegion.b;
                    coordinateChunkPair.Chunk = chunkData;
                    r.ChunkDatas.Add(coordinateChunkPair);
                }
                else
                {
                    r = new RegionData();
                    r.Id = rid;
                    rid++;
                    
                    CoordinateChunkPair coordinateChunkPair = new CoordinateChunkPair();
                    coordinateChunkPair.X = positionInRegion.a;
                    coordinateChunkPair.Y = positionInRegion.b;
                    coordinateChunkPair.Chunk = chunkData;
                    r.ChunkDatas.Add(coordinateChunkPair);
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



        private Chunk AddChunk(Tuple<int, int> gridPosition)
        {
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


        public static bool ChunkLoaded(long chunkid)
        {
            if (instance == null)
            {
                Debug.Log("Instance is null");
                return false;
            }
            else
            {
                Chunk c = GetChunkByID(chunkid);
                if (c == null) return false;
                return c.finishedLoading;
            }
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

        private static Queue<Tuple<bool, ChunkData>> loadQueue = new Queue<Tuple<bool, ChunkData>>();


        public void Update()
        {
            if (LevelManager.currentLevel != null)
            {
                if (loadQueue.Count > 0)
                {
                    var z = loadQueue.Dequeue();
                    _LoadChunk(z.Item2,z.Item1);
                }
            }
        }

        public static void LoadChunk(ChunkData chunkData, bool immediate = true)
        {
            if (LevelManager.currentLevel != null && immediate)
            {
                Debug.Log("Loading Chunk "+chunkData.ChunkId+" immediately");
                _LoadChunk(chunkData);
            }else
            {
                Debug.Log("Added Chunk "+chunkData.ChunkId+" to Load List");
                loadQueue.Enqueue(new Tuple<bool, ChunkData>(immediate,chunkData));
            }
        }


        public static Chunk GetChunkByID(long id)
        {
            foreach (Chunk c in instance.chunks.Values)
            {
                if (c.chunkId == id)
                    return c;
            }

            return null;
        }
        
        // Chunk Semantics
        // Not yet loaded: not in dictionary
        // Currently loading: in dictionary => false
        // Currently loaded: in dictionary => true
        public static void _LoadChunk(ChunkData chunkData, bool immediate = true)
        {
            Debug.Log("Loading Chunk "+chunkData.ChunkId);
            bool loaded;
            if (chunkLoaded.TryGetValue(chunkData.ChunkId, out loaded))
            {
                return;
            }
            else
            {
                chunkLoaded.Add(chunkData.ChunkId, false);
            }

            var levelObjectInstances = new List<LevelObjectInstanceData>();
            levelObjectInstances.AddRange(chunkData.Data.ToList());
            var counter = levelObjectInstances.Count;
            Debug.Log(chunkData.ChunkId + " adding " + counter + " Objects");
            foreach (var i in levelObjectInstances)
            {
                counter--;
                Action Complete = null;
                if (counter == 0)
                {
                    Debug.Log("Adding finish Action for "+chunkData.ChunkId);
                    Complete = () =>
                    {
                        GetChunkByID(chunkData.ChunkId).finishedLoading = true;
                        chunkLoaded[chunkData.ChunkId] = true;
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

        public static LevelObjectInstanceData InstanceToData(InteractiveLevelObject o)
        {
            LevelObjectInstanceData levelObjectInstanceData = InstanceToData((LevelObject) o);
            levelObjectInstanceData.Locations.AddRange(o.receivers.Select((p) =>
            {
                return Util.VectorToLocation(p.Key);
            }));
            return levelObjectInstanceData;
        }
        
        public static LevelObjectInstanceData InstanceToData(LevelObject o)
        {
            LevelObjectInstanceData levelObjectInstanceData = new LevelObjectInstanceData();
            Vector3 pos = o.transform.position;
            Quaternion rot = o.transform.rotation;
            levelObjectInstanceData.X = pos.x;
            levelObjectInstanceData.Y = pos.y;
            levelObjectInstanceData.Z = pos.z;
            levelObjectInstanceData.GX = rot.x;
            levelObjectInstanceData.GY = rot.y;
            levelObjectInstanceData.GZ = rot.z;
            levelObjectInstanceData.GW = rot.w;
            levelObjectInstanceData.Type = o.levelObjectDataType;
            return levelObjectInstanceData;
        }

        public static ChunkData ChunkToData(Chunk chunk)
        {
            ChunkData chunkData = new ChunkData();
            chunkData.ChunkId = chunk.chunkId;
            foreach (LevelObject l in chunk.GetComponentsInChildren<LevelObject>(includeInactive: true))
            {
                chunkData.Data.Add(InstanceToData(l as dynamic));
            }

            return chunkData;
        }
    }
}