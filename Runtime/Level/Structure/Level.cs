using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Level : MonoBehaviour
    {
        public enum LoadType { All, Target };

        public LoadType loadType;

        public Transform dynamicObjects;

        public PlayerSpawn[]    spawn;
        public PlayerGoal		goal;

        bool navigationUpdateNeeded = false;
        LevelNavGenerator navGenerator;

        public LevelObjectData missingPrefab;

        Dictionary<string, LevelObjectData> levelObjectDatas;

        //this needs to be false in network play
        bool _activated;
        public bool activated
        {
            get
            {
                return _activated;
            }
            set
            {
                _activated = value;
                SetLevelObjectActivity(_activated);
            }
        }

        void SetLevelObjectActivity(bool a)
        {
            foreach(LevelObject levelObject in transform.GetComponentsInChildren<LevelObject>())
            {
                levelObject.enabled = a;
            }
        }

        public void Start()
        {
            loadType = LoadType.All;
            Setup();
        }

        public void Setup()
        {
            levelObjectDatas = LevelObjectData.GetAllBuildableByUniqueType();
            dynamicObjects = transform.Find("Dynamic");
            navGenerator = GetComponent<LevelNavGenerator>();
            createPlayerSpawnList();
        }

        public bool PositionOccupied()
        {
            return false;
        }

        public void OnStartRound()
        {
            Setup();
            GenerateNavigation();
            loadType = LoadType.Target;
            SetLevelObjectActivity(true);
            StartLevelObjects();
        }

        public void OnEndRound(bool resetDynamic = true)
        {
            EndLevelObjects();
            SetLevelObjectActivity(false);
            if (resetDynamic) ClearDynamicObjects();
            loadType = LoadType.All;
        }

        void StartLevelObjects()
        {
            foreach(LevelObject o in GetComponentsInChildren<LevelObject>())
            {
                o.OnStartRound();
            }
        }

        void EndLevelObjects()
        {
            foreach (LevelObject o in GetComponentsInChildren<LevelObject>())
            {
                o.OnEndRound();
            }
        }

        void createPlayerSpawnList()
        {
            if (spawn.Length < 4)
                spawn = new PlayerSpawn[4];
        }

        public void GenerateNavigation(bool force = false)
        {
            if(navigationUpdateNeeded || force)
            { 
                navGenerator.UpdateNavMesh();
                navigationUpdateNeeded = false;
            }
        }

        public void ClearDynamicObjects()
        {
            foreach(Transform child in dynamicObjects.transform)
            {
                RemoveDynamic(child.gameObject.GetComponent<LevelObject>());
            }
        }

        public void ResetDynamicState()
        {
            foreach (LevelObject o in GetComponentsInChildren<LevelObject>())
            {
                o.OnDeInit();
                o.OnInit();
            }
        }

        public GameObject Add(LevelObjectInstanceData levelObjectInstanceData)
        {
            GameObject result = null;
            LevelObjectData d;
            if (levelObjectDatas.TryGetValue(levelObjectInstanceData.type, out d))
            {
                if(levelObjectInstanceData.locations.Count > 1)
                {
                    List<Vector3> receiverLocations = levelObjectInstanceData.locations.Select((x) => { return x.ToVector(); }).ToList();
                    result = Add(d, levelObjectInstanceData.GetLocation(), levelObjectInstanceData.GetRotation(),receiverLocations);
                }
                else
                    result = Add(d, levelObjectInstanceData.GetLocation(), levelObjectInstanceData.GetRotation());
            }

            return result;
        }

        // These Objects will be stored in the Chunks and are permanent Information
        // For Interactive Objects only
        public GameObject Add(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation, List<Vector3> receiverLocations)
        {
            GameObject g = Add(levelObjectData,position,rotation);

            InteractiveLevelObject interactiveObject = g.GetComponent<InteractiveLevelObject>();
            foreach(Vector3 receiverlocation in receiverLocations)
            {
                interactiveObject.AddReceiver(receiverlocation);
            }
            //TODO
            return g;
        }
       

        // These Objects will be stored in the Chunks and are permanent Information
        public GameObject Add(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation)
        {
            if (levelObjectData == null || levelObjectData.prefab == null) { return Add(missingPrefab,position,rotation); }

			if(!levelObjectData.levelInstantiable)
			{
				Debug.Log(levelObjectData.name+" cannot be created as constant part of Level");
				return null;
			}

            Chunk chunk = ChunkManager.GetChunk(position);
            if (chunk != null)
            {
                GameObject g = levelObjectData.Create(position, rotation, chunk.transform);
                g.GetComponent<LevelObject>().enabled = activated;
                g.GetComponent<LevelObject>().isDynamic = levelObjectData.dynamicInstantiable;

                navigationUpdateNeeded = true;
                return g;
            }
            return null;
        }

        LevelObject GetTopLevelObject(GameObject g)
        {
            Debug.Log("Getting Top Level of "+g.name);
            LevelObject r = null;
            GameObject p = g;
            LevelObject z = g.GetComponent<LevelObject>();
            do
            {
                r = z;
                var parent = p.transform.parent;
                if (parent == null) break;
                z = parent.gameObject.GetComponentInParent<LevelObject>(includeInactive: true);
                p = p.transform.parent.gameObject;
            } while (z != null);
            return r;
        }

        public void Remove(GameObject levelObject)
        {
            LevelObject toDelete = GetTopLevelObject(levelObject);
            if(toDelete != null)
            {

                if (toDelete.isDynamic)
                {
                    RemoveDynamic(toDelete);
                }

                if (toDelete.transform.parent != dynamicObjects)
                {
                Destroy(toDelete.gameObject);
                navigationUpdateNeeded = true;
                }
            }
        }


       


        //These Objects will be dropped on the next Level reset
        public GameObject AddDynamic(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation)
        {
            if (levelObjectData == null || levelObjectData.prefab == null) {GameObject g = AddDynamic(missingPrefab, position, rotation); return g; }
            if (!levelObjectData.dynamicInstantiable)
			{
				Debug.Log(levelObjectData.name+" cannot be created dynamically");
				return null;
			}
            var created = levelObjectData.Create(position, rotation, dynamicObjects);
            created.GetComponent<LevelObject>().enabled = activated;
            created.GetComponent<LevelObject>().isDynamic = levelObjectData.dynamicInstantiable;
            return created;
        }

        public void AddToDynamic(GameObject g)
        {
            g.transform.position = transform.position;
            g.transform.parent = dynamicObjects;
            g.GetComponent<LevelObject>().isDynamic = true;
        }
        public void AddToDynamic(GameObject g, Vector3 position, Quaternion rotation)
        {
            g.transform.position = position;
            g.transform.rotation = rotation;
            g.transform.parent = dynamicObjects;
            g.GetComponent<LevelObject>().isDynamic = true;
        }

        public void RemoveDynamic(LevelObject o, bool physics = true)
        {
            if(o.isDynamic)
            {
                if (physics)
                {
                    Destroy(o.gameObject);
                }
                else
                {
                    DestroyImmediate(o.gameObject);
                }
            }
        }

        public List<LevelObject> GetAllDynamicLevelObjects(bool inactive = true)
        {
            List<LevelObject> dynamicObjects = transform.GetComponentsInChildren<LevelObject>(includeInactive: inactive).ToList();
            dynamicObjects = dynamicObjects.FindAll( (x) => x.isDynamic == true ).ToList();
            return dynamicObjects;
        }

        public LevelObject GetLevelObjectAt(Vector3 position)
        {
            Chunk targetChunk = ChunkManager.GetChunk(position,createIfNotThere: false);
            if (targetChunk == null) return null;
            return targetChunk.GetLevelObjectAt(position);
        }
    }
}
