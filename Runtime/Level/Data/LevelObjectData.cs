using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelObjectData", order = 1)]
    public class LevelObjectData : Instantiable
    {
        public static List<LevelObjectData> all = new List<LevelObjectData>();

        public TileLevelObjectData tileLevelObjectData;
        
        public ushort uniqueLevelObjectId;

        public string levelObjectName;
        
        [Header("Attributes")]

        public Constants.Color color;

        public ElementType elementType = ElementType.defaultElementType;
        
        [Header("Level Placement Settings")]
        
        public float granularity;

        public bool dynamicInstantiable;

        public bool levelInstantiable;

        public bool ActivateWhenInactive;

        public bool Tiled;
        
        public bool Buildable;

        public bool inLevel = true;
        
        public int maximumNumber;
        
        [Header("Preview Settings")]
       
        public Mesh cursorMesh;
        public Vector3 cursorScale;
        public Vector3 cursorOffset;
        public Vector3 cursorRotation;

        
        [System.Serializable]
        public class Property {
            [SerializeField]
            public string name;

            string _value;
            
            public string Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                    valueChanged.Invoke();
                }
            }

            public UnityEvent valueChanged = new UnityEvent();
            [SerializeField]
            // T must be encodable as a string
            public string defaultValue;
        }
		
		
        public List<Property> properties;
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Buildable) levelInstantiable = true;

            if (cursorScale == Vector3.zero) cursorScale = new Vector3(1, 1, 1);
#endif
        }

        public void OnEnable()
        {
            UpdateMesh();
        }

        public static LevelObjectData[] GetAllBuildable()
        {
            var data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            return data.FindAll(x => x.Buildable).ToArray();
        }

        public static Dictionary<int, LevelObjectData> GetAllByUniqueType()
        {
            var data = new List<LevelObjectData>(Resources.LoadAll<LevelObjectData>("LevelObjectData"));
            var dict = new Dictionary<int, LevelObjectData>();
            foreach (var objectData in data) dict.Add(objectData.uniqueLevelObjectId, objectData);
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


        public void UpdateMesh()
        {
            if(cursorMesh == null)
            {
                if(prefab != null)
                {
                    GameObject g = null;
                    try
                    {
                        GameConsole.Log($"Instantiating {levelObjectName}",false);
                        g = Instantiate(prefab) as GameObject;
                        Destroy(g.GetComponent<LevelObject>());
                        g.SetActive(false);
                        MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>(includeInactive: true);
                        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                        int i = 0;
                        while (i < meshFilters.Length)
                        {
                            combine[i].mesh = meshFilters[i].sharedMesh;
                            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                            meshFilters[i].gameObject.SetActive(false);
                            i++;
                        }
                        cursorMesh = new Mesh();
                    cursorMesh.CombineMeshes(combine);
                    }
            catch (Exception e)
            {
                if (Application.isPlaying)
                {
                  
                    Destroy(g);
                }
                else
                {
                    DestroyImmediate(g);  
                }
                
                Console.WriteLine(e);
                return;
            }
            GameConsole.Log("Removing "+g);
            if (Application.isPlaying)
            {
                Destroy(g);
            }
            else
            {
                DestroyImmediate(g);  
            }
            }
            }
        }

        public Mesh GetMesh()
        {
            if (cursorMesh == null)
            {
                UpdateMesh();
            }
            return cursorMesh;
        }
        
        
        public override GameObject Create(Vector3 location, Quaternion rotation, Transform parent,Util.Optional<int> identity)
        {
            
            var g = base.Create(location, rotation, parent);
            var lO = g.GetComponent<LevelObject>();
            
            
            lO.levelObjectDataType = uniqueLevelObjectId;
            lO.elementType = elementType;
            lO.color = color;
            
            
            if (identity != null && identity.IsSet())
            {
                    
                NetworkLevelObject networkLevelObject = g.GetComponent<NetworkLevelObject>();
                if (networkLevelObject != null)
                {
                    networkLevelObject.Identity = identity.Get();
                }else
                {
                    GameConsole.Log($"Tried to pre set identity {identity.Get()} but {g} is not a NetworkLevelObject");
                }
            }

            List<Property> newProperties = properties.Select((x) =>
            {
                var r = new Property
                {
                    name = x.name,
                    Value = x.Value,
                    defaultValue = x.defaultValue
                };
                return r;
            }).ToList();
            
            lO.levelObjectProperties = newProperties;
            lO.OnInit();
            return g;
        }
    }
}
