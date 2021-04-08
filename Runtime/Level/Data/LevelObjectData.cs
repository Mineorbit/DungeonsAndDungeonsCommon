using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		

    }


}
