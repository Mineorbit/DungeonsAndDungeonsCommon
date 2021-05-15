using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EnemySpawn : Spawn
    {
        public LevelObjectData EnemyToSpawn;

        public Vector3 spawnOffset;

        GameObject spawnedEnemy;

        int maxSpawnCount = 1;

        int spawnCount = 1;

        public override void OnStartRound()
        {
            bool full_collider = Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test;
            GetComponent<Collider>().enabled = full_collider;
            GetComponent<Collider>().isTrigger = !full_collider;
            Setup();
        }

        public override void OnEndRound()
        {
            bool full_collider = Level.instantiateType == Level.InstantiateType.Play || Level.instantiateType == Level.InstantiateType.Test;
            GetComponent<Collider>().enabled = full_collider;
            GetComponent<Collider>().isTrigger = !full_collider;
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
            if(!(Level.instantiateType == Level.InstantiateType.Online))
            { 
            if (spawnedEnemy == null)
            {
                Vector3 spawnLocation = SpawnLocation();
                Quaternion spawnRotation = SpawnRotation();
                spawnedEnemy = LevelManager.currentLevel.AddDynamic(EnemyToSpawn, spawnLocation, spawnRotation);
                spawnedEnemy.GetComponent<Entity>().Spawn(spawnLocation, spawnRotation,true);
            }
            else
            {
                LevelManager.currentLevel.AddToDynamic(spawnedEnemy, SpawnLocation(), SpawnRotation());
            }
            }
        }

    }
}
