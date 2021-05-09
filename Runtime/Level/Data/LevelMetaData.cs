using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
	[Serializable]
    public class LevelMetaData
    {
        public int localLevelId;
        public long uniqueLevelId;
		public string FullName;
		public string Description;
		
		public bool availBlue;
		public bool availYellow;
		public bool availRed;
		public bool availGreen;
        
		public LevelMetaData(string name){
		FullName = name;
		localLevelId = 0;
		uniqueLevelId = 0;
		}


		public static LevelMetaData Load(string path)
        {
			// Add file safety check
			SaveManager m = new SaveManager(SaveManager.StorageType.JSON);
			LevelMetaData metaData = m.Load<LevelMetaData>(path);
			return metaData;
        }
		public void Save(string path)
        {
			availBlue = LevelManager.currentLevel.spawn[0] != null;
			availYellow = LevelManager.currentLevel.spawn[1] != null;
			availRed = LevelManager.currentLevel.spawn[2] != null;
			availGreen = LevelManager.currentLevel.spawn[3] != null;

			SaveManager m = new SaveManager(SaveManager.StorageType.JSON);
			m.Save(this,path, persistent: false);
        }
		public static LevelMetaData FromNetData(NetLevel.LevelMetaData netData)
        {
			LevelMetaData metaData = new LevelMetaData(netData.FullName);
			metaData.Description = netData.Description;
			metaData.localLevelId = netData.LocalLevelId;
			metaData.uniqueLevelId = netData.UniqueLevelId;
			metaData.availBlue = netData.AvailBlue;
			metaData.availGreen = netData.AvailGreen;
			metaData.availRed = netData.AvailRed;
			metaData.availYellow = netData.AvailYellow;
			return metaData;
        }
    }
}
