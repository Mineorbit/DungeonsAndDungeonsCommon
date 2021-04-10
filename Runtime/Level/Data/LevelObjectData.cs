using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.Generic;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelObjectData", order = 1)]
    public class LevelObjectData : Instantiable
    {
        public enum Orientation { North = 0, East = 1, South = 2, West = 3};
        
        public int levelObjectDataId;

        public float granularity;

        public string name;
		
		public bool dynamicInstantiable;
		
		public bool levelInstantiable;
		

        public static LevelObjectData[] GetAllBuildable()
        {
            List<LevelObjectData> data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            Debug.Log(data);
            return data.FindAll(x => x.levelInstantiable).ToArray();
        }

    }


}
