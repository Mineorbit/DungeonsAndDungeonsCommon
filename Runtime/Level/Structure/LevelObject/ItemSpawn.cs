using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ItemSpawn : LevelObject
    {
        public LevelObjectData itemToSpawn;

        public Vector3 spawnOffset;

        GameObject spawnedItem;

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
            SpawnItem();
        }

        //Change to on remove

        public void OnDestroy()
        {
            RemoveSpawnedItem();
        }

        void RemoveSpawnedItem()
        {

            if (spawnedItem != null)
            {
                LevelManager.currentLevel.RemoveDynamic(spawnedItem.GetComponent<Item>());
            }
        }

        public override void OnInit()
        {
            Setup();
        }

        public override void OnDeInit()
        {
            RemoveSpawnedItem();
        }

        Vector3 SpawnLocation()
        {
            return transform.position + spawnOffset;
        }

        public void SpawnItem()
        {
            Debug.Log("Spawned Item");
            if (spawnedItem == null)
            {
                spawnedItem = LevelManager.currentLevel.AddDynamic( itemToSpawn, SpawnLocation(), new Quaternion(0, 0, 0, 0));
            }
            else
            {
                spawnedItem.transform.position = SpawnLocation();
            }
        }

    }
}
