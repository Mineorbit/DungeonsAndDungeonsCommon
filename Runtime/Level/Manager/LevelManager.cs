using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelManager : MonoBehaviour
    {

        public static UnityEngine.Object levelPrefab;

        public static LevelMetaData currentLevelMetaData;
        
        public static Level currentLevel;


        public void Start()
        {
            LevelDataManager.New();
        }

        public static void Instantiate()
        {
            if (levelPrefab == null) levelPrefab = Resources.Load("Level") as UnityEngine.Object;
            GameObject level = Instantiate(levelPrefab) as GameObject;
            level.name = "Level " + currentLevelMetaData.localLevelId;
            currentLevel = level.GetComponent<Level>();
        }

        

        
        public static void Reset()
        {
            LevelMetaData lastMetaData = currentLevelMetaData;
            Clear();
            LevelDataManager.Load(lastMetaData);
        }
		
		public static void StartRound()
		{
			Reset();
			currentLevel.OnStartRound();
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
