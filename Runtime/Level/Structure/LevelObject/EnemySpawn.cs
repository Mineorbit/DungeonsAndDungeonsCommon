using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemySpawn : LevelObject
    {
        public LevelObjectData EnemyToSpawn;

        public Vector3 spawnOffset;

        GameObject spawnedEnemy;

        int maxSpawnCount = 1;

        int spawnCount = 1;

        public override void OnStartRound()
        {
            GetComponent<Collider>().enabled = false;
            Setup();
        }

        public override void OnEndRound()
        {
            GetComponent<Collider>().enabled = true;
            Setup();
        }

        void Setup()
        {
            spawnCount = maxSpawnCount;
            SpawnEnemy();
        }

        //Change to on remove

        public void OnDestroy()
        {
            if (spawnedEnemy != null) LevelManager.currentLevel.RemoveDynamic(spawnedEnemy.GetComponent<Enemy>());
        }


        public override void OnInit()
        {
            Setup();
        }

        public override void OnDeInit()
        {
            Setup();
        }

        Vector3 SpawnLocation()
        {
            return transform.position + spawnOffset;
        }

        public void SpawnEnemy()
        {
            Debug.Log("Spawned Enemy");
            if (spawnedEnemy == null)
            {
                spawnedEnemy = LevelManager.currentLevel.AddDynamic(EnemyToSpawn, SpawnLocation(), new Quaternion(0, 0, 0, 0));
            }
            else
            {
                spawnedEnemy.transform.position = SpawnLocation();
            }
        }

    }
}
