using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Level : MonoBehaviour
    {

        public enum InstantiateType { Default, Test, Online, Edit,Play}

        static InstantiateType _instantiateType;

        public static InstantiateType instantiateType
        {
            get
            {
                return _instantiateType;
            }
            set
            {
                Debug.Log("Updated Instantiate Type to: "+value);
                if (value == InstantiateType.Test)
                {
                    activated = true;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.None;
                }
                if(value == InstantiateType.Online)
                {
                    activated = false;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.Near;
                }
                if (value == InstantiateType.Play)
                {
                    Debug.Log("HOLLLA");
                    activated = true;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.Near;
                }
                if (value == InstantiateType.Default)
                {
                    activated = true;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.None;
                }
                if(value == InstantiateType.Edit)
                {
                    activated = false;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.None;
                }
                _instantiateType = value;
            }
        }


        public Transform dynamicObjects;

        public PlayerSpawn[]    spawn;
        public PlayerGoal		goal;

        bool navigationUpdateNeeded = false;
        LevelNavGenerator navGenerator;

        public LevelObjectData missingPrefab;


        //this needs to be false in network play
        static bool _activated;
        public static bool activated
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



        static void SetLevelObjectActivity(bool a)
        {
            if(LevelManager.currentLevel != null)
            foreach(LevelObject levelObject in LevelManager.currentLevel.transform.GetComponentsInChildren<LevelObject>())
            {
                levelObject.enabled = a || levelObject.ActivateWhenInactive;
            }
        }

        public void Start()
        {
            Setup();
        }

        public void Setup()
        {
            Debug.Log("Setting up Level");
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
            SetLevelObjectActivity(true);
            StartLevelObjects();
        }

        public void OnEndRound(bool resetDynamic = true)
        {
            EndLevelObjects();
            SetLevelObjectActivity(false);
            if (resetDynamic) ClearDynamicObjects();
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
            if (LevelDataManager.levelObjectDatas.TryGetValue(levelObjectInstanceData.type, out d))
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
                g.GetComponent<LevelObject>().enabled = activated||levelObjectData.ActivateWhenInactive;
                g.GetComponent<LevelObject>().isDynamic = levelObjectData.dynamicInstantiable;
                g.GetComponent<LevelObject>().ActivateWhenInactive = levelObjectData.ActivateWhenInactive;

                navigationUpdateNeeded = true;
                return g;
            }
            return null;
        }

        LevelObject GetTopLevelObject(GameObject g)
        {
            LevelObject r = null;
            GameObject p = g;
            if (g == null) return null;
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
            Debug.Log(rotation.eulerAngles);
            var created = levelObjectData.Create(position, rotation, dynamicObjects);
            created.GetComponent<LevelObject>().enabled = activated || levelObjectData.ActivateWhenInactive;
            created.GetComponent<LevelObject>().isDynamic = levelObjectData.dynamicInstantiable;
            created.GetComponent<LevelObject>().ActivateWhenInactive = levelObjectData.ActivateWhenInactive;
            return created;
        }

        public void AddToDynamic(GameObject g)
        {
            Vector3 position = g.transform.position;
            g.transform.parent = dynamicObjects;
            g.transform.position = position;
            g.GetComponent<LevelObject>().isDynamic = true;
        }
        public void AddToDynamic(GameObject g, Vector3 position, Quaternion rotation)
        {
            g.transform.parent = dynamicObjects;
            g.transform.position = position;
            g.transform.rotation = rotation;
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
