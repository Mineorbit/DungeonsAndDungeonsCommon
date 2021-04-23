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
            SpawnItem();
        }

        public override void OnEndRound()
        {
            GetComponent<Collider>().enabled = true;
            SpawnItem();
        }

        public override void OnInit()
        {
            SpawnItem();
        }

        public override void OnEnable()
        {
            spawnCount = maxSpawnCount;
            SpawnItem();
        }

        public override void OnDisable()
        {
            spawnCount = maxSpawnCount;
            SpawnItem();
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

        void SpawnItem()
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
            else
            {
                spawnedItem.transform.position = SpawnLocation();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
