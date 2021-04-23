using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                o.Reset();
            }
        }


       

        //These Objects will be stored in the Chunks and are permanent Information
        public void Add(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation)
        {
            if (levelObjectData == null || levelObjectData.prefab == null) { Add(missingPrefab,position,rotation); return; }

			if(!levelObjectData.levelInstantiable)
			{
				Debug.Log(levelObjectData.name+" cannot be created as constant part of Level");
				return;
			}

            Chunk chunk = ChunkManager.GetChunk(position);
            if (chunk != null)
            {
                GameObject g = levelObjectData.Create(position, rotation, chunk.transform);
                g.GetComponent<LevelObject>().enabled = activated;

                navigationUpdateNeeded = true;
            }
        }

        LevelObject GetTopLevelObject(GameObject g)
        {
            LevelObject r = null;
            GameObject p = g;
            LevelObject z = g.GetComponent<LevelObject>();
            do
            {
                r = z;
                z = p.transform.parent.gameObject.GetComponentInParent<LevelObject>(includeInactive: true);
                p = p.transform.parent.gameObject;
            } while (z != null);
            return r;
        }

        public void Remove(GameObject levelObject)
        {

            LevelObject toDelete = GetTopLevelObject(levelObject);

            if(toDelete != null)
            {

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
            return created;
        }

        public void AddToDynamic(GameObject g)
        {
            g.transform.position = transform.position;
            g.transform.parent = dynamicObjects;
        }

        public void RemoveDynamic(LevelObject o)
        {
            if(o.transform.parent == dynamicObjects)
            {
                Debug.Log("Removing dynamic LevelObject"+o.gameObject.name);
                Destroy(o.gameObject);
            }
        }

        public Transform GetAllDynamicObjects()
        {
            return dynamicObjects;
        }
    }
}
