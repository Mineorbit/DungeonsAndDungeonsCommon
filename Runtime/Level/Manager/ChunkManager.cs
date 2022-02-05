using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using NetLevel;
using UnityEngine;
using UnityEngine.Events;
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

        public static float chunkGranularity = 8f;

        public static int regionGranularity = 2;

        private static readonly ProtoSaveManager regionDataLoader = new ProtoSaveManager();

        public Dictionary<Tuple<int, int, int>, Chunk> chunks;
        
        public static Dictionary<string, bool> chunkLoaded = new Dictionary<string, bool>();

        private bool ready;

        public Dictionary<int, bool> regionLoaded = new Dictionary<int, bool>();
        
        public static UnityEvent onChunkLoaded = new UnityEvent();
        
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
                chunkLoaded = new Dictionary<string, bool>();
            }
            chunkLoaded.Clear();
                chunkPrefab = Resources.Load("Chunk");
                chunks = new Dictionary<Tuple<int, int, int>, Chunk>();
                
        }
        
        public static void LoadRegion(int regionId, LoadType loadType = LoadType.Disk)
        {
            var regionData = LoadRegionData(regionId, loadType);
            if (regionData == null) return;
            foreach (var chunkLocationPair in regionData.ChunkDatas) LoadChunk(chunkLocationPair.Chunk);
            instance.regionLoaded[regionId] = true;
        }

        public static RegionData LoadRegionData(Tuple<int, int, int> chunkPosition)
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


        public Dictionary<Tuple<int, int, int>, RegionData> FetchRegionData()
        {
            var regionData = new Dictionary<Tuple<int, int, int>, RegionData>();
            var rid = 0;
            foreach (var chunk in chunks)
            {
                var regionPosition = GetRegionPosition(chunk.Key);
                var positionInRegion = GetPositionInRegion(chunk.Key);

                ChunkData chunkData = ChunkManager.ChunkToData(chunk.Value);


                GameConsole.Log("Chunk at " + chunk.Key + " goes to Region " + regionPosition);


                RegionData r;

                if (regionData.TryGetValue(regionPosition, out r))
                {
                    CoordinateChunkPair coordinateChunkPair = new CoordinateChunkPair();
                    coordinateChunkPair.X = positionInRegion.a;
                    coordinateChunkPair.Y = positionInRegion.b;
                    coordinateChunkPair.Z = positionInRegion.c;
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
                    coordinateChunkPair.Z = positionInRegion.c;
                    coordinateChunkPair.Chunk = chunkData;
                    r.ChunkDatas.Add(coordinateChunkPair);
                    regionData.Add(regionPosition, r);
                }
            }

            return regionData;
        }

        private (int a, int b, int c) GetPositionInRegion(Tuple<int, int, int> chunkPosition)
        {
            return (chunkPosition.Item1 % regionGranularity, chunkPosition.Item2 % regionGranularity, chunkPosition.Item3 % regionGranularity);
        }

        private static Tuple<int, int, int> GetRegionPosition(Tuple<int, int, int> chunkPosition)
        {
            var x = (int) Mathf.Floor(chunkPosition.Item1 / (float) regionGranularity);
            var y = (int) Mathf.Floor(chunkPosition.Item2 / (float) regionGranularity);
            var z = (int) Mathf.Floor(chunkPosition.Item3 / (float) regionGranularity);
            var regionPosition = new Tuple<int, int, int>(x, y, z);
            return regionPosition;
        }

        private static char CIDSEP = '|';

        public static Vector3 GetChunkPosition(string chunkID)
        {
            string[] s = chunkID.Split(CIDSEP);
            Vector3 pos = new Vector3(Int32.Parse(s[0]) * chunkGranularity
                , Int32.Parse(s[1]) * chunkGranularity, Int32.Parse(s[2]) * chunkGranularity);
            return pos - new Vector3(1, 1, 1)*chunkGranularity/2;
        }

        private Chunk AddChunk(Tuple<int, int, int> gridPosition)
        {
            var chunk = Instantiate(chunkPrefab) as GameObject;
            chunk.transform.parent = LevelManager.currentLevel.transform;
            chunk.transform.position = ChunkPositionFromGridPosition(gridPosition);
            var c = chunk.GetComponent<Chunk>();
            c.chunkId = GetChunkID(gridPosition);
            chunks.Add(gridPosition, c);
            return c;
        }

	// THIS NEEDS OVERHAUL

        public static string GetChunkID(Tuple<int, int, int> gridPosition)
        {
            int[] result = new int[] {gridPosition.Item1,gridPosition.Item2, gridPosition.Item3};
            string id = String.Join(CIDSEP+"", result);
            return id;
        }


        public static  Tuple<int, int, int> GetChunkGridByID(string id)
        {
            string[] coords = id.Split(CIDSEP);
            var t = new Tuple<int, int, int>(Int32.Parse(coords[0]),Int32.Parse(coords[1]),Int32.Parse(coords[2]) );
            GameConsole.Log($"{id} to {t}");
            return t;
        }
        
        public static bool ChunkLoaded(string chunkid)
        {
            if (instance == null)
            {
                GameConsole.Log("Instance is null");
                return false;
            }
            else
            {
                Chunk c = GetChunkByID(chunkid);
                if (c == null) return false;
                return c.finishedLoading;
            }
        }

        public static Vector3 ChunkPositionFromGridPosition(Tuple<int, int, int> gridPosition)
        {
            return new Vector3(gridPosition.Item1 * chunkGranularity, gridPosition.Item2 * chunkGranularity, gridPosition.Item3 * chunkGranularity) -
                   chunkGranularity / 2 * new Vector3(1, 1, 1);
        }

        public static Tuple<int, int, int> GetChunkGridPosition(Vector3 position)
        {
            return new Tuple<int, int, int>((int) Mathf.Floor(position.x / chunkGranularity),
            				(int) Mathf.Floor(position.y / chunkGranularity),
                			(int) Mathf.Floor(position.z / chunkGranularity));
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

        public static Chunk[] GetNeighborhood(Vector3 position)
        {
            Chunk[] neighborhood = new Chunk[27];
            var grid = GetChunkGridPosition(position);
            int c = 0;
            for (int i = -1; i<2;i++)
            	for (int j = -1; j<2;j++)
            		for (int k = -1; k<2;k++)
            		{
            			Chunk neighborChunk = GetChunkByID(GetChunkID(new Tuple<int, int, int>(grid.Item1 + i, grid.Item2 + j,grid.Item3 + k)));
                        if (neighborChunk != null)
                            neighborhood[c] = neighborChunk;

                        c++;
                    }
            return neighborhood;
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
                GameConsole.Log("Loading Chunk "+chunkData.ChunkId+" immediately");
                _LoadChunk(chunkData);
            }else
            {
                GameConsole.Log("Added Chunk "+chunkData.ChunkId+" to Load List");
                loadQueue.Enqueue(new Tuple<bool, ChunkData>(immediate,chunkData));
            }
        }

// WZF PASSIERT HIER
        public static Chunk GetChunkByID(string id)
        {
            Chunk result = null;
            if(LevelManager.currentLevel != null)
            {
                Chunk[] chunks = LevelManager.currentLevel.GetComponentsInChildren<Chunk>(includeInactive: true);

                foreach (Chunk c in chunks)
                {
                    if(c != null)
                    {
                        if (c.chunkId == id)
                        {
                            result = c;
                        }
                    }
                }
            }
            return result;
        }
        
        // Chunk Semantics
        // Not yet loaded: not in dictionary
        // Currently loading: in dictionary => false
        // Currently loaded: in dictionary => true
        public static void _LoadChunk(ChunkData chunkData, bool immediate = true)
        {
            GameConsole.Log("Loading Chunk "+chunkData.ChunkId);
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
            foreach (var instanceData in levelObjectInstances)
            {
                counter--;
                Action Complete = null;
                if (counter == 0)
                {
                    Complete = () =>
                    {
                        if(chunkData != null)
                        {
                            Chunk c = GetChunkByID(chunkData.ChunkId);
                            if(c != null)
                            {
                                c.finishedLoading = true;
                            }
                            else
                            {
                                GameConsole.Log("Chunk was null");
                            }
                            
                            chunkLoaded[chunkData.ChunkId] = true;
                            onChunkLoaded.Invoke();
                            
                        }
                        else
                        {
                            GameConsole.Log("Chunk Data was null");
                        }
                    };
                }

                Vector3 chunkPosition = GetChunkPosition(chunkData.ChunkId);
                LevelObjectInstance instance = LevelObjectInstanceDataToLevelObjectInstance(instanceData, chunkPosition);  
                
                if (immediate)
                {
                    LevelManager.currentLevel.Add(instance);
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
                            LevelManager.currentLevel.Add(instance);
                            Complete.Invoke();
                        }); 
                    }
                    else
                    {
                        MainCaller.Do(() =>
                        {
                            LevelManager.currentLevel.Add(instance);
                        });  
                    }
                    

                    
                
                }
            }
            
        }
        
        
        
        
       
       public static float storageMultiplier = 2f;
	
        public static LevelObjectInstanceData InstanceToData(InteractiveLevelObject o, Chunk c)
        {
            LevelObjectInstanceData levelObjectInstanceData = InstanceToData((LevelObject) o, c);
            // THIS IS NOT GOOD AND MUST BE REPLACED LATER ON
            levelObjectInstanceData.Identity = o.Identity;
            levelObjectInstanceData.Receivers.AddRange(o.receivers.Keys);
            return levelObjectInstanceData;
        }
        
        public static LevelObjectInstanceData InstanceToData(LevelObject o, Chunk c)
        {
            LevelObjectInstanceData levelObjectInstanceData = new LevelObjectInstanceData();
            Vector3 pos = storageMultiplier*(o.transform.position - c.transform.position);
            Quaternion rot = o.transform.rotation;
            levelObjectInstanceData.X = (byte) pos.x;
            levelObjectInstanceData.Y = (byte) pos.y;
            levelObjectInstanceData.Z = (byte) pos.z;
            levelObjectInstanceData.Rot = (byte) Mathf.Floor(rot.eulerAngles.y / 90);
            levelObjectInstanceData.Code = o.levelObjectDataType;
            return levelObjectInstanceData;
        }

        


        public static LevelObjectInstance LevelObjectInstanceDataToLevelObjectInstance(LevelObjectInstanceData instanceData, Vector3 offset)
        {
            Quaternion rot = Quaternion.Euler(0,90*instanceData.Rot,0);
            LevelObjectInstance levelObjectInstance = new LevelObjectInstance();
            levelObjectInstance.X = (1/storageMultiplier)*(instanceData.X) + offset.x;
            levelObjectInstance.Y = (1/storageMultiplier)*(instanceData.Y) + offset.y;
            levelObjectInstance.Z = (1/storageMultiplier)*(instanceData.Z) + offset.z;
            levelObjectInstance.GX = rot.x;
            levelObjectInstance.GY = rot.y;
            levelObjectInstance.GZ = rot.z;
            levelObjectInstance.GW = rot.w;
            levelObjectInstance.Identity = instanceData.Identity;
            levelObjectInstance.Receivers.AddRange(instanceData.Receivers);
            levelObjectInstance.Type = instanceData.Code;
            return levelObjectInstance;
        }

        public static ChunkData ChunkToData(Chunk chunk)
        {
            LevelObject[] instances = chunk.GetComponentsInChildren<LevelObject>(includeInactive: true);
            ChunkData chunkData = new ChunkData();
            chunkData.ChunkId = chunk.chunkId;
            foreach (LevelObject l in instances)
            {
                chunkData.Data.Add(InstanceToData(l as dynamic, chunk));
            }

            return chunkData;
        }
        
        public static byte[] DataToBinary(ChunkData chunkData)
        {
            byte[] data = new byte[1024];
            GameConsole.Log($"{chunkData}");
            Vector3 offset = GetChunkPosition(chunkData.ChunkId);

            foreach (LevelObjectInstanceData instanceData in chunkData.Data)
            {
                ushort elementType = (ushort) instanceData.Code;

                float localX =  instanceData.X - offset.x;
                float localY = instanceData.Y - offset.y;
                float localZ = instanceData.Z - offset.z;
                GameConsole.Log($"{instanceData} is local as {localX} {localY} {localZ}");
                
                int z = 64*((int) localX) + 8*((int) localY) +((int) localZ);
                int i = 2 * z;
                byte upper = (byte) (elementType >> 8);
                byte lower = (byte) (elementType & 0xff);

                upper = (byte) ((byte) (upper & 0x3f) | (byte)((byte)instanceData.Rot << 6));
                
                data[i] = upper;
                data[i + 1] = lower;
            }
            return data;
        }

        public static ChunkData BinaryToData(byte[] data, Tuple<int, int, int> gridPosition)
        {
            ChunkData chunkData = new ChunkData();
            Vector3 offset = GetChunkPosition(GetChunkID(gridPosition));
            for(int i = 0;i < 8;i++)
                for(int j = 0;j<8;j++)
                    for (int k = 0; k < 8; k++)
                    {
                        int z = 64*((int) i) + 8*((int) j) +((int) k);
                        int d = 2 * z;
                        byte upper = data[d];
                        byte lower = data[d+1];
                        byte upper2 = (byte) ((byte) (upper & 0x3f));
                        ushort elementType = (ushort) ( ((int) upper2) * 256 + (int)lower);
                        if(elementType != 0)
                        { 
                            int rot = upper >> 6;
                            LevelObjectInstanceData objectData = new LevelObjectInstanceData();
                            objectData.Code = elementType;
                            objectData.X = (uint) (i + offset.x);
                            objectData.Y = (uint) (j + offset.y);
                            objectData.Z = (uint) (k + offset.z);
                            objectData.Rot = (uint) rot;
                            chunkData.Data.Add(objectData);
                        }
                    }
            GameConsole.Log($"{chunkData}");
            return chunkData;
        }
        
    }
}
