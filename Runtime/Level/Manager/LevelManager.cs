using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public static UnityEvent levelEndedEvent;

        public void Start()
        {
            levelStartedEvent = new UnityEvent();
            levelEndedEvent = new UnityEvent();
        }


        public static void Instantiate()
        {
            if (levelPrefab == null) levelPrefab = Resources.Load("Level") as UnityEngine.Object;
            GameObject level = Instantiate(levelPrefab) as GameObject;
            level.name = "Level " + currentLevelMetaData.localLevelId;
            currentLevel = level.GetComponent<Level>();
            currentLevel.Setup();
            ChunkManager cm = level.GetComponent<ChunkManager>();
            cm.Setup();
        }


        public static void ResetDynamicObjects()
        {
            currentLevel.ClearDynamicObjects();
        }

        public static void ResetDynamicState()
        {
            Debug.Log("Resetting dynamic LevelObjects");
            if (currentLevel != null)
            {
                currentLevel.ResetDynamicState();
            }
            else Debug.Log("Currently there is no Level instantiated");
        }

        public static void ResetToSaveState()
        {
            LevelMetaData lastMetaData = currentLevelMetaData;
            Level.InstantiateType lastInstantiateType = Level.instantiateType;
            Clear();
            LevelDataManager.Load(lastMetaData,lastInstantiateType);
        }

        public static void StartRound(bool resetDynamic = true, bool resetStatic = false)
        {
                if (resetDynamic)
                    ResetDynamicObjects();
                if(resetStatic)
                    ResetToSaveState();
            if (currentLevel != null)
            {
                currentLevel.OnStartRound();
                levelStartedEvent.Invoke();
            }
            else Debug.Log("Currently there is no Level instantiated");
        }

        public static void EndRound(bool resetDynamic = true)
        {
            currentLevel.OnEndRound(resetDynamic: resetDynamic);
            levelEndedEvent.Invoke();
        }

        public static Vector3 GetGridPosition(Vector3 exactPosition, LevelObjectData levelObjectData)
        {
            float g = 1;
            if (levelObjectData == null && levelObjectData.granularity != 0)
                g = levelObjectData.granularity;

            return g*(new Vector3(Mathf.Round(exactPosition.x/g), Mathf.Round(exactPosition.y / g), Mathf.Round(exactPosition.z / g)));
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
