using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Level : MonoBehaviour
    {
        enum LevelState {Active , Inactive};
        enum LoadType { All, Target };
        LevelState levelState;
        LoadType loadType;

        public Transform dynamicObjects;

        public PlayerSpawn[]    spawn;
        public PlayerGoal		goal;

      

        public void Start()
        {
            levelState = LevelState.Inactive;
            loadType = LoadType.All;
            Setup();
        }
		
        public void OnStartRound()
        {
            //GenerateNavigation(); Change generation to different interval
            levelState = LevelState.Active;
            loadType = LoadType.Target;
            Setup();
        }
        void Setup()
        {
            dynamicObjects = transform.Find("Dynamic");
            createPlayerSpawnList();
        }
        void createPlayerSpawnList()
        {
            if (spawn.Length < 4)
                spawn = new PlayerSpawn[4];
        }

        void GenerateNavigation()
        {

        }

        void ClearDynamicObjects()
        {
            foreach(Transform child in dynamicObjects)
            {
                Destroy(child.gameObject);
            }
        }


        void OnEndRound()
        {
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
            if(chunk != null)
            levelObjectData.Create(position, rotation,chunk.transform);

        }

		public void Remove(LevelObject levelObjectData)
        {
            if(levelObjectData.transform.parent != dynamicObjects)
            {
                Destroy(levelObjectData.gameObject);
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
            Debug.Log("HALLO");
            return levelObjectData.Create(position, rotation, dynamicObjects);
        }

        public void RemoveDynamic(LevelObject o)
        {
            if(o.transform.parent == dynamicObjects)
            {
                Destroy(o.gameObject);
            }
        }

        public static GameObject[] GetAllEnemies()
        {
            return new GameObject[0];
        }
    }
}
