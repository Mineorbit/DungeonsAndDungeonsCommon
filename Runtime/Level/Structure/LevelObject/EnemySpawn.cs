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

        


        //Change to on remove

        public void OnDestroy()
        {
            if (spawnedEnemy != null) LevelManager.currentLevel.RemoveDynamic(spawnedEnemy.GetComponent<Enemy>());
        }


        Vector3 SpawnLocation() 
        {
            return transform.position + spawnOffset;
        }

        public void SpawnEnemy()
        {
            if (spawnedEnemy == null)
            {
                spawnedEnemy = LevelManager.currentLevel.AddDynamic(EnemyToSpawn, SpawnLocation(), new Quaternion(0, 0, 0, 0));
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
