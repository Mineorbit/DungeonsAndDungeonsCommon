using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemySpawn : LevelObject
    {
        public LevelObjectData enemyToSpawn;

        public int maxSpawnCount = 1;

        public Vector3 spawnOffset;

        int spawnCount;

        List<GameObject> spawnedEnemies;



        public override void OnInit()
        {
            if(spawnedEnemies.Count == 1)
            {
                spawnedEnemies[0].transform.position = SpawnLocation();
            }
        }

        public override void OnEnable()
        {
			if(spawnedEnemies == null)
            spawnedEnemies = new List<GameObject>();
            spawnCount = maxSpawnCount;
            SpawnEnemy();
        }

		
		public override void OnDeInit()
		{
		    foreach(GameObject g in spawnedEnemies)
		    {
				LevelManager.currentLevel.RemoveDynamic(g.GetComponent<Enemy>());
		    }
		}
        Vector3 SpawnLocation()
        {
            return transform.position + spawnOffset;
        }
		
        void SpawnEnemy()
        {
            if(spawnCount > 0)
            {
                spawnCount--;
                Debug.Log("spawning it");
                GameObject newEnemy = LevelManager.currentLevel.AddDynamic(enemyToSpawn,SpawnLocation(),new Quaternion(0,0,0,0));
                Debug.Log(newEnemy);
                if(newEnemy != null)
                {
                    spawnedEnemies.Add(newEnemy);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
