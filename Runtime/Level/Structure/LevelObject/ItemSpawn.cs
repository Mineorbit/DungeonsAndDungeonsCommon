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

        GameObject spawnedItem;

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
            spawnedItem.transform.position = SpawnLocation();
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
            if (spawnedItem != null) LevelManager.currentLevel.RemoveDynamic(spawnedItem.GetComponent<Item>()); 
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
            if (spawnedItem == null)
            {
                Debug.Log("spawning it");
                spawnedItem = LevelManager.currentLevel.AddDynamic( itemToSpawn, SpawnLocation(), new Quaternion(0, 0, 0, 0));
                if (spawnedItem != null)
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
