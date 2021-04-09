using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelManager : MonoBehaviour
    {

        public static UnityEngine.Object levelPrefab;

        public static LevelMetaData currentLevelMetaData;

        public static Level currentLevel;


        public static UnityEvent levelStartedEvent;

        public void Start()
        {
            levelStartedEvent = new UnityEvent();
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

        public static void StartRound(bool reset = true)
        {
            if (reset) Reset();
            currentLevel.OnStartRound();
            levelStartedEvent.Invoke();
        }

        public static void Clear()
        {
            if (currentLevel != null)
            {
                Destroy(currentLevel.gameObject);
                currentLevelMetaData = null;
            }
        }
    }
}
