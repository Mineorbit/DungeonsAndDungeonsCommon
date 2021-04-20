using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Level : MonoBehaviour
    {
        public enum LevelState {Active , Inactive};
        public enum LoadType { All, Target };

        public LevelState levelState;
        public LoadType loadType;

        public Transform dynamicObjects;

        public PlayerSpawn[]    spawn;
        public PlayerGoal		goal;

        bool navigationUpdateNeeded = false;
        LevelNavGenerator navGenerator;


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
            levelState = LevelState.Inactive;
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
            GenerateNavigation();
            levelState = LevelState.Active;
            loadType = LoadType.Target;
            Setup();
            SetLevelObjectActivity(true);
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
                Destroy(child.gameObject);
            }
        }


        public void OnEndRound()
        {
            SetLevelObjectActivity(false);
            ClearDynamicObjects();
            levelState = LevelState.Inactive;
            loadType = LoadType.All;
        }

        //These Objects will be stored in the Chunks and are permanent Information
        public void Add(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation)
        {
            if (levelObjectData == null) return;

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

        LevelObject GetTopLevelObject()
        {
            LevelObject toDelete = null;
            LevelObject p = levelObject.GetComponent<LevelObject>(includeinactive: true);
            do
            {
                toDelete = p;
                p = levelObject.transform.parent.gameObject.GetComponentInParent<LevelObject>(includeinactive: true);
            } while (p != null);
            return toDelete;
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
            if (levelObjectData == null) return null;
			if(!levelObjectData.dynamicInstantiable)
			{
				Debug.Log(levelObjectData.name+" cannot be created dynamically");
				return null;
			}
            var created = levelObjectData.Create(position, rotation, dynamicObjects);
            created.GetComponent<LevelObject>().enabled = activated;
            return created;
        }

        public void RemoveDynamic(LevelObject o)
        {
            if(o.transform.parent == dynamicObjects)
            {
                Destroy(o.gameObject);
            }
        }

        public Transform GetAllDynamicObjects()
        {
            return dynamicObjects;
        }
    }
}
