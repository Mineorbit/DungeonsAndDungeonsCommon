using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemySpawn : LevelObject
    {
        public LevelObjectData enemyToSpawn;

        public int maxSpawnCount = 1;

        int spawnCount;

        List<GameObject> spawnedEnemies;



        public override void OnInit()
        {
			if(spawnedEnemies == null)
            spawnedEnemies = new List<GameObject>();
            spawnCount = maxSpawnCount;
            SpawnEnemy();
        }

        public override void OnEnable()
        {
			if(spawnedEnemies == null)
            spawnedEnemies = new List<GameObject>();
            spawnCount = maxSpawnCount - spawnedEnemies.Count;
            if(spawnCount == maxSpawnCount) SpawnEnemy();
        }
		
		public override void OnDisable()
		{
		foreach(GameObject g in spawnedEnemies)
		{
				LevelManager.currentLevel.RemoveDynamic(g.GetComponent<Enemy>());
		}
		}
		
        void SpawnEnemy()
        {
            if(spawnCount > 0)
            {
                spawnCount--;
                GameObject newEnemy = LevelManager.currentLevel.AddDynamic(enemyToSpawn,transform.position+new Vector3(0,0.5f,0),new Quaternion(0,0,0,0));
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
