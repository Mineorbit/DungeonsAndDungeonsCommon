using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemySpawn : LevelObject
    {
        public LevelObjectData EnemyToSpawn;

        public int maxSpawnCount = 1;

        public Vector3 spawnOffset;

        int spawnCount;

        GameObject spawnedEnemy;

        public override void OnStartRound()
        {
            GetComponent<Collider>().enabled = false;
            SpawnEnemy();
        }

        public override void OnEndRound()
        {
            GetComponent<Collider>().enabled = true;
            SpawnEnemy();
        }

        public override void OnInit()
        {
            SpawnEnemy();
        }

        public override void OnEnable()
        {
            spawnCount = maxSpawnCount;
            SpawnEnemy();
        }

        public override void OnDisable()
        {
            spawnCount = maxSpawnCount;
            SpawnEnemy();
        }


        //Change to on remove

        public void OnDestroy()
        {
            if (spawnedEnemy != null) LevelManager.currentLevel.RemoveDynamic(spawnedEnemy.GetComponent<Enemy>());
        }



        public override void OnDeInit()
        {
        }

        Vector3 SpawnLocation() 
        {
            return transform.position + spawnOffset;
        }

        void SpawnEnemy()
        {
            if (spawnedEnemy == null)
            {
                Debug.Log("spawning it");
                spawnedEnemy = LevelManager.currentLevel.AddDynamic(EnemyToSpawn, SpawnLocation(), new Quaternion(0, 0, 0, 0));
                if (spawnedEnemy != null)
                {
                    spawnCount--;
                }
            }
            else
            {
                spawnedEnemy.transform.position = SpawnLocation();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
