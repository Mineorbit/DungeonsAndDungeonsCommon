using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelObjectData", order = 1)]
    public class LevelObjectData : Instantiable
    {
        public static List<LevelObjectData> all = new List<LevelObjectData>();
        public int uniqueLevelObjectId;

        public float granularity;

        public string name;

        public bool dynamicInstantiable;

        public bool levelInstantiable;

        public bool ActivateWhenInactive;

        public bool Buildable;

        public Mesh cursorMesh;

        public Vector3 cursorScale;
        public Vector3 cursorOffset;
        public Vector3 cursorRotation;


        void AssignUniqueNumbers()
        {
            if(uniqueLevelObjectId == 0)
            uniqueLevelObjectId = GUID.Generate().GetHashCode();
        }
        private void OnValidate()
        {
#if UNITY_EDITOR
            AssignUniqueNumbers();
            if (Buildable) levelInstantiable = true;

            if (cursorScale == Vector3.zero) cursorScale = new Vector3(1, 1, 1);
#endif
        }

        public static LevelObjectData[] GetAllBuildable()
        {
            var data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            return data.FindAll(x => x.Buildable).ToArray();
        }

        public static Dictionary<int, LevelObjectData> GetAllByUniqueType()
        {
            Debug.Log("TEST A");
            var data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            Debug.Log("TEST D");
            var dict = new Dictionary<int, LevelObjectData>();
            foreach (var objectData in data) dict.Add(objectData.uniqueLevelObjectId, objectData);
            Debug.Log("TEST B");
            return dict;
        }

        public static Dictionary<int, LevelObjectData> GetAllBuildableByUniqueType()
        {
            var data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            data = data.FindAll(x => x.Buildable);
            var dict = new Dictionary<int, LevelObjectData>();
            foreach (var objectData in data) dict.Add(objectData.uniqueLevelObjectId, objectData);
            return dict;
        }


        public Mesh GetMesh()
        {
            return cursorMesh;
        }

        public override GameObject Create(Vector3 location, Quaternion rotation, Transform parent)
        {
            var g = base.Create(location, rotation, parent);
            var lO = g.GetComponent<LevelObject>();
            lO.levelObjectDataType = uniqueLevelObjectId;
            lO.OnInit();
            return g;
        }
    }
}