using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelDataManager : MonoBehaviour
    {

		public static LevelMetaData[] localLevels;
		public static LevelMetaData[] networkLevels;
		
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

    }
}
