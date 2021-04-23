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
        bool inRound;

        public override void OnStartRound()
        {
            GetComponent<Collider>().enabled = false;
            SpawnItem();
            inRound = true;
        }

        public override void OnEndRound()
        {
            GetComponent<Collider>().enabled = true;
            SpawnItem();
            inRound = false;
        }


        //Change to on remove

        public void OnDestroy()
        {
            if (spawnedItem != null) LevelManager.currentLevel.RemoveDynamic(spawnedItem.GetComponent<Item>()); 
        }

        void Update()
        {
            if(!inRound && spawnedItem == null)
            {
                SpawnItem();
            }
        }

        public override void OnDeInit()
        {
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
