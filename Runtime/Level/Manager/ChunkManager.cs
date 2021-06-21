using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
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

        public static List<Chunk> GetNeighborhood(Vector3 position)
        {
            List<Chunk> neighborhood = new List<Chunk>();
            var grid = GetChunkGridPosition(position);
            neighborhood.Add(GetChunkByID(GetChunkID(grid)));
            Chunk c1 = GetChunkByID(GetChunkID(new Tuple<int, int>(grid.Item1 + 1, grid.Item2)));
            if(c1 != null)
            neighborhood.Add(c1);
            Chunk c2 = GetChunkByID(GetChunkID(new Tuple<int, int>(grid.Item1 - 1, grid.Item2)));
            if(c2 != null)
            neighborhood.Add(c2);
            Chunk c3 = GetChunkByID(GetChunkID(new Tuple<int, int>(grid.Item1, grid.Item2 + 1)));
            if(c3 != null)
            neighborhood.Add(c3);
            Chunk c4 = GetChunkByID(GetChunkID(new Tuple<int, int>(grid.Item1, grid.Item2 - 1)));
            if(c4 != null)
            neighborhood.Add(c4);
            Debug.Log("Found in Neighborhood "+neighborhood.Count);
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
                Debug.Log("Loading Chunk "+chunkData.ChunkId+" immediately");
                _LoadChunk(chunkData);
            }else
            {
                Debug.Log("Added Chunk "+chunkData.ChunkId+" to Load List");
                loadQueue.Enqueue(new Tuple<bool, ChunkData>(immediate,chunkData));
            }
        }

// WZF PASSIERT HIER
        public static Chunk GetChunkByID(long id)
        {
            Chunk result = null;
            if(LevelManager.currentLevel != null)
            {
                Chunk[] chunks = LevelManager.currentLevel.GetComponentsInChildren<Chunk>(includeInactive: true);

                foreach (Chunk c in chunks)
                {
                Debug.Log("There is Chunk "+c.chunkId+" looking for "+id);
                if(c != null)
                {
                if (c.chunkId == id)
                {
                    Debug.Log("Match bei "+id+" ist Chunk: "+c);
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
            foreach (var instanceData in levelObjectInstances)
            {
                counter--;
                Action Complete = null;
                if (counter == 0)
                {
                    Debug.Log("Adding finish Action for "+chunkData.ChunkId);
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
                                Debug.Log("Chunk was null");
                            }
                            
                            chunkLoaded[chunkData.ChunkId] = true;
                        }
                        else
                        {
                            Debug.Log("Chunk Data was null");
                        }
                    };
                }

                LevelObjectInstance instance = LevelObjectInstanceDataToLevelObjectInstance(instanceData, chunkData.HuffmanTable.ToList());  
                
                if (immediate)
                {
                    Debug.Log("Adding "+instance);
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
                            Debug.Log("Adding " + instance);
                            LevelManager.currentLevel.Add(instance);
                        });  
                    }
                    

                    
                
                }
            }
        }
        class HuffNode
        {
            public HuffNode l;
            public HuffNode r;
            public HuffNode p;
            public int type = 0;
            public float w = 0;

            public int CompareTo(HuffNode other)
            {
                if (w <= other.w)
                {
                    return 1;
                }
                return 0;
            }
        }
        class HuffNodeComparer : IComparer<HuffNode>
        {
            public int Compare(HuffNode x, HuffNode y)
            {
                // TODO: Handle x or y being null, or them not having names
                return x.CompareTo(y);
            }
        }

        class PriorityQueue
        {
            private List<HuffNode> values = new List<HuffNode>();
            public void Enqueue(HuffNode d)
            {
                values.Add(d);
                values.Sort( new HuffNodeComparer());
            }
            
            public HuffNode Dequeue()
            {
                var v = values.First();
                values.RemoveAt(0);
                return v;
            }

            public int Count()
            {
                return values.Count;
            }
        }
        
        public static List<HuffmanEntry> BuildHuffmanTable(List<LevelObject> levelObjects)
        {
            Dictionary<int, float> weights = new Dictionary<int, float>();
            foreach (LevelObject l in levelObjects)
            {
                float freq;
                if(weights.TryGetValue(l.levelObjectDataType, out freq))
                {
                    weights[l.levelObjectDataType] = freq + 1;
                }else
                {
                    weights.Add(l.levelObjectDataType,1);
                }
            }

            float count = levelObjects.Count;

            PriorityQueue todo = new PriorityQueue();
            Dictionary<int, HuffNode> nodes = new Dictionary<int, HuffNode>();

            foreach (var type in weights.Keys)
            {
                
                float weight = weights[type] / count;
                Debug.Log("Weight: "+type+" "+weight);
                HuffNode huffNode = new HuffNode();
                huffNode.type = type;
                huffNode.w = weight;
                todo.Enqueue(huffNode);
                nodes.Add(type,huffNode);
            }

            while (1 < todo.Count())
            {
                var h1 = todo.Dequeue();
                var h2 = todo.Dequeue();
                HuffNode huffNode = new HuffNode();
                huffNode.l = h1;
                huffNode.r = h2;
                huffNode.w = h1.w + h2.w;
                h1.p = huffNode;
                h2.p = huffNode;
                todo.Enqueue(huffNode);
            }
            
            HuffNode root = todo.Dequeue();
            List<HuffmanEntry> huffmanEntries = new List<HuffmanEntry>();
            foreach (var type in weights.Keys)
            {
                List<bool> pathToType = BuildPath(nodes[type]);
                BitArray b = new BitArray(pathToType.ToArray());
                int numOfBytes = (int) Math.Ceiling((double) pathToType.Count / 8);
                byte[] bytes = new byte[numOfBytes];
                b.CopyTo(bytes,0);
                HuffmanEntry huffmanEntry = new HuffmanEntry();
                ByteString s = ByteString.CopyFrom(bytes);
                huffmanEntry.Code = s;
                huffmanEntry.Type = type;
                huffmanEntries.Add(huffmanEntry);
            }

            return huffmanEntries;
        }

        private static List<bool> BuildPath(HuffNode node)
        {
            List<bool> path = new List<bool>();
            if(node.p != null)
            {
            path = BuildPath(node.p);
            if (node == node.p.l)
            {
                path.Add(true);
            }
            else
            {
                path.Add(false);
            }
            
            }

            return path;
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
            levelObjectInstanceData.Code = TypeToCode(o.levelObjectDataType);
            return levelObjectInstanceData;
        }

        private static List<HuffmanEntry> currentChunkHuffmanTable;
        public static int CodeToType(ByteString code,  List<HuffmanEntry> huffmanEntries)
        {
            foreach (var x in huffmanEntries)
            {
                if (x.Code.Equals(code))
                {
                    return x.Type;
                }
            }

            // HIER SPÄTER Type VON ERROR BLOCK
            return 0;
        }

        public static int CodeToType(ByteString code)
        {
            return CodeToType(code, currentChunkHuffmanTable);
        }


        public static ByteString TypeToCode(int type)
        {
            return TypeToCode(type, currentChunkHuffmanTable);
        }
        
        public static ByteString TypeToCode(int type, List<HuffmanEntry> huffmanEntries)
        {
            foreach (var x in huffmanEntries)
            {
                if (x.Type.Equals(type))
                {
                    return x.Code;
                }
            }

            // HIER SPÄTER Code VON ERROR BLOCK
            return ByteString.Empty;
        }


        public static LevelObjectInstance LevelObjectInstanceDataToLevelObjectInstance(LevelObjectInstanceData instanceData, List<HuffmanEntry> huffmanEntries)
        {
            LevelObjectInstance levelObjectInstance = new LevelObjectInstance();
            levelObjectInstance.X = instanceData.X;
            levelObjectInstance.Y = instanceData.Y;
            levelObjectInstance.Z = instanceData.Z;
            levelObjectInstance.GX = instanceData.GX;
            levelObjectInstance.GY = instanceData.GY;
            levelObjectInstance.GZ = instanceData.GZ;
            levelObjectInstance.GW = instanceData.GW;
            levelObjectInstance.Locations.AddRange( instanceData.Locations );
            levelObjectInstance.Type = CodeToType(instanceData.Code, huffmanEntries);
            return levelObjectInstance;
        }

        public static ChunkData ChunkToData(Chunk chunk)
        {
            LevelObject[] instances = chunk.GetComponentsInChildren<LevelObject>(includeInactive: true);
            ChunkData chunkData = new ChunkData();
            chunkData.ChunkId = chunk.chunkId;
            if(instances.Length > 0)
            currentChunkHuffmanTable = BuildHuffmanTable(instances.ToList());
            chunkData.HuffmanTable.AddRange(currentChunkHuffmanTable);
            foreach (LevelObject l in instances)
            {
                chunkData.Data.Add(InstanceToData(l as dynamic));
            }

            return chunkData;
        }
    }
}