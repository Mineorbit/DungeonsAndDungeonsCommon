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
			SaveManager m = new SaveManager(SaveManager.StorageType.JSON);
			m.Save(this,path, persistent: false);
        }
    }
}
