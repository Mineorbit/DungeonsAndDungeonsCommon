using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelDataManager : MonoBehaviour
    {

		public static LevelMetaData[] localLevels;
		public static LevelMetaData[] networkLevels;
		
		

		public static void New(LevelMetaData levelMetaData)
        {
            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
			LevelManager.Instantiate();
			Save();
        }
		public static void New()
        {
            LevelMetaData newMetaData = GetNewLevelMetaData();
			New(newMetaData);
        }
		
		public static void Load(LevelMetaData levelMetaData)
        {
            if (levelMetaData == null) return;
            LevelManager.currentLevelMetaData = levelMetaData;
            LevelManager.Instantiate();
        }
		
		
		
        public static void Save()
        {

        }
		
		public static void Delete(LevelMetaData metaData)
		{
		}
		
        public static LevelMetaData GetNewLevelMetaData()
        {
            LevelMetaData levelMetaData = LevelMetaData.CreateInstance<LevelMetaData>();
            levelMetaData.localLevelId = GetFreeLocalId();
            return levelMetaData;
        }

        static int GetFreeLocalId()
        {
            return 1;
        }
		
		public static void UpdateLocalLevelList(){
		
		}

    }
}
