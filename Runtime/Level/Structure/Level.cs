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

        Transform dynamicObjects;
		
		public PlayerSpawn[] 	spawn;
		public PlayerGoal		goal;

        void Start()
        {
            levelState = LevelState.Inactive;
            loadType = LoadType.Target;
            dynamicObjects = transform.Find("Dynamic");
        }
		
        public void OnStartRound()
        {
            //GenerateNavigation(); Change generation to different interval
            levelState = LevelState.Active;
            loadType = LoadType.Target;
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


            Chunk chunk = ChunkManager.GetChunk(position);

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
        public void AddDynamic(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation)
        {
            if (levelObjectData == null) return;
            levelObjectData.Create(position, rotation, dynamicObjects);
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
