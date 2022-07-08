using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Chunk : MonoBehaviour
    {
        public string chunkId;
        private readonly float eps = 0.05f;

        public bool finishedLoading = false;

        public UnityEngine.Object chunkSystemPrefab;


        public Dictionary<int, CubeTileGrid> tileSystems = new Dictionary<int, CubeTileGrid>();
        
        private void Start()
        {
            gameObject.name = "Chunk " + chunkId;
            GameConsole.Log("Chunk "+chunkId+" created");
        }

        public void OnDestroy()
        {
            GameConsole.Log("Chunk "+chunkId+" removed");
        }

        public CubeTileGrid SetupTileSystemFor(LevelObjectData d)
        {
            GameObject tileSystem = Instantiate(chunkSystemPrefab,transform) as GameObject;
            CubeTileGrid s = tileSystem.GetComponent<CubeTileGrid>();
            s.Init();
            tileSystems.Add(d.uniqueLevelObjectId, s);
            return s;
        }
        
        public LevelObject GetLevelObjectAt(Vector3 position)
        {
            foreach (LevelObject child in transform.GetComponentsInChildren<LevelObject>(includeInactive: true))
            {
                if ((child.transform.position - position).magnitude < eps)
                {
                    return child;
                }
                
            }
            return null;
        }

        public (int, int, int) GetLocalCoordinates(Vector3 position)
        {
            Vector3 diff = position - transform.position;
            return ((int) diff.x, (int) diff.y, (int) diff.z);
        }
        public void AddTiledLevelObject(LevelObjectData levelObjectData,Vector3 position)
        {
            CubeTileGrid tileSystem;
            if (!tileSystems.TryGetValue(levelObjectData.uniqueLevelObjectId, out tileSystem))
            {
                tileSystem = SetupTileSystemFor(levelObjectData);
            }

            var (a, b, c) = GetLocalCoordinates(position);
            tileSystem.AddCube(a,b,c);
        }
    }

}
