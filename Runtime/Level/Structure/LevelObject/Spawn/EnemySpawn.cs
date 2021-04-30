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
        }

        void Setup()
        {
            spawnCount = maxSpawnCount;
            SpawnEnemy();
        }

        //Change to on remove

        public void OnDestroy()
        {
            RemoveSpawnedEnemy();
        }

        void RemoveSpawnedEnemy()
        {
            Debug.Log("Trying to Remove");

            if (spawnedEnemy != null)
            {
                LevelManager.currentLevel.RemoveDynamic(spawnedEnemy.GetComponent<Enemy>(),physics: false);
            }
        }

        public override void OnInit()
        {
            base.OnInit();
            Setup();
        }

        public override void OnDeInit()
        {
            base.OnDeInit();
            RemoveSpawnedEnemy();
        }

        Vector3 SpawnLocation()
        {
            return transform.position + spawnOffset;
        }

        Quaternion SpawnRotation()
        {

            Debug.Log(" Set Rotation to "+transform.rotation.eulerAngles);
            return transform.rotation;
        }

        public void SpawnEnemy()
        {
            if (spawnedEnemy == null)
            {
                spawnedEnemy = LevelManager.currentLevel.AddDynamic(EnemyToSpawn, SpawnLocation(), SpawnRotation());
            }
            else
            {
                LevelManager.currentLevel.AddToDynamic(spawnedEnemy, SpawnLocation(), SpawnRotation());
            }
        }

    }
}
