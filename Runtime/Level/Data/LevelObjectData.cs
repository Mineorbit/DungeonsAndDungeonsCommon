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

        public string uniqueLevelObjectId;

        public float granularity;

        public string name;
		
		public bool dynamicInstantiable;
		
		public bool levelInstantiable;
		
        void OnValidate()
        {
#if UNITY_EDITOR
            if(uniqueLevelObjectId == "")
            {
            uniqueLevelObjectId = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        public static LevelObjectData[] GetAllBuildable()
        {
            List<LevelObjectData> data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            return data.FindAll(x => x.levelInstantiable).ToArray();
        }

        public static Dictionary<string,LevelObjectData> GetAllBuildableByUniqueType()
        {
            List<LevelObjectData> data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            data = data.FindAll(x => x.levelInstantiable);
            Dictionary<string, LevelObjectData> dict = new Dictionary<string, LevelObjectData>();
            foreach(LevelObjectData objectData in data)
            {
                dict.Add(objectData.uniqueLevelObjectId,objectData);
            }
            return dict;
        }


        public override GameObject Create(Vector3 location, Quaternion rotation, Transform parent)
        {
            GameObject g = base.Create(location, rotation, parent);
            LevelObject lO = g.GetComponent<LevelObject>();
            lO.levelObjectDataType = uniqueLevelObjectId;
            lO.OnInit();
            return g;
        }

    }

}
