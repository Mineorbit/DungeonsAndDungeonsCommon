using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetLevel;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Level : MonoBehaviour
    {
        public enum InstantiateType
        {
            Default,
            Test,
            Online,
            Edit,
            Play
        }

        private static InstantiateType _instantiateType;


        //this needs to be false in network play
        private static bool _activated;
        
        public Transform dynamicObjects;

        public PlayerSpawn[] spawn;
        public PlayerGoal goal;

        public LevelObjectData missingPrefab;


        private bool isSetup;
        private LevelNavGenerator navGenerator;

        private bool navigationUpdateNeeded;

        public static InstantiateType instantiateType
        {
            get => _instantiateType;
            set
            {
                if(LevelDataManager.instance != null)
                {
                    GameConsole.Log("Updated Instantiate Type to: " + value);
                if (value == InstantiateType.Test)
                {
                    activated = true;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.None;
                }

                if (value == InstantiateType.Online)
                {
                    activated = false;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.Near;
                }

                if (value == InstantiateType.Play)
                {
                    activated = true;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.Near;
                }

                if (value == InstantiateType.Default)
                {
                    activated = true;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.None;
                }

                if (value == InstantiateType.Edit)
                {
                    activated = false;
                    LevelDataManager.instance.loadType = LevelDataManager.LoadType.All;
                    LevelLoadTarget.loadTargetMode = LevelLoadTarget.LoadTargetMode.None;
                }
                
                }
                _instantiateType = value;
            }
        }

        public static bool activated
        {
            get => _activated;
            set
            {
                _activated = value;
                SetLevelObjectActivity(_activated);
            }
        }

        private static void SetLevelObjectActivity(bool a)
        {
            if (LevelManager.currentLevel != null)
                foreach (var levelObject in LevelManager.currentLevel.transform.GetComponentsInChildren<LevelObject>())
                    levelObject.enabled = a || levelObject.ActivateWhenInactive;
        }

        public void Setup()
        {
            if (!isSetup)
            {
                isSetup = true;
                Debug.Log("Setting up Level");
                dynamicObjects = transform.Find("Dynamic");
                navGenerator = GetComponent<LevelNavGenerator>();
                createPlayerSpawnList();
                CreateEnqueuedEntities();
            }
        }

        public static Queue<Action> entityCreation = new Queue<Action>();
        public void CreateEnqueuedEntities()
        {
            while (entityCreation.Count>0)
            {
                entityCreation.Dequeue().DynamicInvoke();
            }
        }
        public bool PositionOccupied()
        {
            return false;
        }

        public void OnStartRound()
        {
            Setup();
            GenerateNavigation();
            SetLevelObjectActivity(true);
            StartLevelObjects();
        }

        public void OnDestroy()
        {
            ResetIdentitifiedObjects();
        }

        public void OnEndRound(bool resetDynamic = true)
        {
            EndLevelObjects();
            SetLevelObjectActivity(false);
            if (resetDynamic) ClearDynamicObjects();
        }

        private void StartLevelObjects()
        {
            foreach (var o in GetComponentsInChildren<LevelObject>()) o.OnStartRound();
        }

        private void EndLevelObjects()
        {
            foreach (var o in GetComponentsInChildren<LevelObject>()) o.OnEndRound();
        }

        private void createPlayerSpawnList()
        {
            if (spawn.Length < 4)
                spawn = new PlayerSpawn[4];
        }

        public void GenerateNavigation(bool force = false)
        {
            if (navigationUpdateNeeded || force)
            {
                navGenerator.UpdateNavMesh();
                navigationUpdateNeeded = false;
            }
        }

        public void ClearDynamicObjects()
        {
            foreach (Transform child in dynamicObjects.transform)
                RemoveDynamic(child.gameObject.GetComponent<LevelObject>());
        }

        public void ResetDynamicState()
        {
            foreach (var o in GetComponentsInChildren<LevelObject>())
            {
                o.OnDeInit();
                o.OnInit();
            }
        }

        public void ResetIdentitifiedObjects(bool exceptPlayers = true)
        {
            // NetworkLevelObject.identifiedLevelObjects.Clear();
            // foreach ()
            // THIS IS STOLEN FROM STACKOVERFLOW AN SHOULD BE REPLACED
            if(exceptPlayers)
            {
                var toRemove = LevelManager.currentLevel.GetComponentsInChildren<NetworkLevelObject>()
                    .Select((x) =>  x.Identity);
                foreach (int r in toRemove)
                    NetworkLevelObject.identifiedLevelObjects.Remove(r);
            }else
            {
                NetworkLevelObject.identifiedLevelObjects.Clear();
            }
        }
        
        public GameObject Add(LevelObjectInstance levelObjectInstance)
        {
            GameObject result = null;
            LevelObjectData d;
            if (LevelDataManager.levelObjectDatas.TryGetValue(levelObjectInstance.Type, out d))
            {
                Vector3 pos = new Vector3(levelObjectInstance.X, levelObjectInstance.Y, levelObjectInstance.Z);
                Quaternion rot = new Quaternion(levelObjectInstance.GX, levelObjectInstance.GY, levelObjectInstance.GZ,levelObjectInstance.GW);
                

				Util.Optional<int> id = new Util.Optional<int>();
				if(levelObjectInstance.Identity != 0)
                	id.Set(levelObjectInstance.Identity);
                result = Add(d, pos, rot, id);

                InteractiveLevelObject interactiveLevelObject = result.GetComponent<InteractiveLevelObject>();
                if(interactiveLevelObject != null)
                {
                	foreach(int identity in levelObjectInstance.Receivers)
                	{
                		GameConsole.Log("Adding Receiver "+identity+" from LevelObjectInstance");
                		interactiveLevelObject.AddReceiver(identity);
                	}
                }
            }
            else
            {
                Debug.Log("Could not find LevelObjectData for "+levelObjectInstance.Type);
            }

            return result;
        }
        
        
        // These Objects will be stored in the Chunks and are permanent Information
        public GameObject Add(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation, Util.Optional<int> identity)
        {
            if (levelObjectData == null || levelObjectData.prefab == null)
                return Add(missingPrefab, position, rotation,identity);

            if (!levelObjectData.levelInstantiable)
            {
                Debug.Log(levelObjectData.levelObjectName + " cannot be created as constant part of Level");
                return null;
            }

            var chunk = ChunkManager.GetChunk(position);
            if (chunk != null)
            {
                var g = levelObjectData.Create(position, rotation, chunk.transform,identity);
                LevelObject levelObject = g.GetComponent<LevelObject>();
                levelObject.enabled = activated || levelObjectData.ActivateWhenInactive;
                levelObject.isDynamic = levelObjectData.dynamicInstantiable;
                levelObject.ActivateWhenInactive = levelObjectData.ActivateWhenInactive;
                
                navigationUpdateNeeded = true;
                return g;
            }

            return null;
        }

        private static LevelObject GetTopLevelObject(GameObject g)
        {
            LevelObject r = null;
            var p = g;
            if (g == null) return null;
            var z = g.GetComponent<LevelObject>();
            do
            {
                r = z;
                var parent = p.transform.parent;
                if (parent == null) break;
                z = parent.gameObject.GetComponentInParent<LevelObject>(true);
                p = p.transform.parent.gameObject;
            } while (z != null);

            return r;
        }

        public void Remove(GameObject levelObject)
        {
            var toDelete = GetTopLevelObject(levelObject);
            if (toDelete != null)
            {
                if (toDelete.isDynamic) RemoveDynamic(toDelete);

                if (toDelete.transform.parent != dynamicObjects)
                {
                    Destroy(toDelete.gameObject);
                    navigationUpdateNeeded = true;
                }
            }
        }
        


        //These Objects will be dropped on the next Level reset
        public GameObject AddDynamic(LevelObjectData levelObjectData, Vector3 position, Quaternion rotation, Util.Optional<int> identity)
        {
            if (levelObjectData == null || levelObjectData.prefab == null)
            {
                var g = AddDynamic(missingPrefab, position, rotation, identity);
                return g;
            }

            if (!levelObjectData.dynamicInstantiable)
            {
                Debug.Log(levelObjectData.levelObjectName + " cannot be created dynamically");
                return null;
            }

            GameObject created = levelObjectData.Create(position, rotation, dynamicObjects,identity);
            LevelObject levelObject = created.GetComponent<LevelObject>();
            levelObject.enabled = activated || levelObjectData.ActivateWhenInactive;
            levelObject.isDynamic = levelObjectData.dynamicInstantiable;
            levelObject.ActivateWhenInactive = levelObjectData.ActivateWhenInactive;
            
            
            
            return created;
        }

        public void AddToDynamic(GameObject g)
        {
            var position = g.transform.position;
            g.transform.parent = dynamicObjects;
            g.transform.position = position;
            g.GetComponent<LevelObject>().isDynamic = true;
        }

        public void AddToDynamic(GameObject g, Vector3 position, Quaternion rotation)
        {
            g.transform.parent = dynamicObjects;
            g.transform.position = position;
            g.transform.rotation = rotation;
            g.GetComponent<LevelObject>().isDynamic = true;
        }

        public void RemoveDynamic(LevelObject o, bool physics = true)
        {
            if (o.isDynamic)
            {
                if (physics)
                    Destroy(o.gameObject);
                else
                    DestroyImmediate(o.gameObject);
            }
        }

        public List<LevelObject> GetAllDynamicLevelObjects(bool inactive = true)
        {
            var dynamicObjects = transform.GetComponentsInChildren<LevelObject>(inactive).ToList();
            dynamicObjects = dynamicObjects.FindAll(x => x.isDynamic).ToList();
            return dynamicObjects;
        }

        public LevelObject GetLevelObjectAt(Vector3 position)
        {
            LevelObject found = null;
            // Temporary fix
            Chunk[] neighborhood = ChunkManager.GetNeighborhood(position);
            int i = 0;
            foreach(Chunk targetChunk in neighborhood)
            {
                
            if (targetChunk != null)
            {
                found = targetChunk.GetLevelObjectAt(position);
                if (found != null)
                {
                    return found;
                }
            }
            
            }

            return null;
        }
    }
}
