using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelMetaData : ScriptableObject
    {
        public int localLevelId;
        public long uniqueLevelId;
		public string FullName;
        
		public LevelMetaData(string name){
		FullName = name;
		localLevelId = 0;
		uniqueLevelId = 0;
		}
    }
}
