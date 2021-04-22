using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ItemSpawn : LevelObject
    {
        public LevelObjectData itemToSpawn;

        public int maxSpawnCount = 1;

        public Vector3 spawnOffset;

        int spawnCount;

        GameObject spawnedEnemy;

        public override void OnStartRound()
        {
            GetComponent<Collider>().enabled = false;
        }

        public override void OnEndRound()
        {
            GetComponent<Collider>().enabled = true;
        }

        public override void OnInit()
        {
            spawnedEnemy.transform.position = SpawnLocation();
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
                spawnedEnemy = LevelManager.currentLevel.AddDynamic( itemToSpawn, SpawnLocation(), new Quaternion(0, 0, 0, 0));
                if (spawnedEnemy != null)
                {
                    spawnCount--;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
