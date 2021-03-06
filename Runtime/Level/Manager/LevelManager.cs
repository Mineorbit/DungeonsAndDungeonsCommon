using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelManager : MonoBehaviour
    {

        public static UnityEngine.Object levelPrefab;

        static LevelMetaData currentLevelMetaData;
        
        public static Level currentLevel;


        public void Start()
        {
            New();
        }

        static void Instantiate()
        {
            if (levelPrefab == null) levelPrefab = Resources.Load("Level") as UnityEngine.Object;
            GameObject level = Instantiate(levelPrefab) as GameObject;
            level.name = "Level " + currentLevelMetaData.localLevelId;
            currentLevel = level.GetComponent<Level>();
        }

        public static void New()
        {
            Clear();
            currentLevelMetaData = LevelDataManager.GetNewLevelMetaData();
            Instantiate();
            ChunkManager.GetChunk(new Vector3(0,0,58));
        }


        public static void Load(LevelMetaData levelMetaData)
        {
            if (levelMetaData == null) return;
            currentLevelMetaData = levelMetaData;
            Instantiate();
        }
        public static void Save()
        {

        }
        public static void Reset()
        {
            LevelMetaData lastMetaData = currentLevelMetaData;
            Clear();
            Load(lastMetaData);
        }
        public static void Clear()
        {
            if(currentLevel!=null)
            {
                Destroy(currentLevel.gameObject);
                currentLevelMetaData = null;
            }
        }
    }
}
