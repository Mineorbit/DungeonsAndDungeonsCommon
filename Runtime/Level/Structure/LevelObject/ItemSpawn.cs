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
        }

        void Setup()
        {
            spawnCount = maxSpawnCount;
            SpawnItem();
        }

        //Change to on remove

        public void OnDestroy()
        {
            if (spawnedItem.GetComponent<Item>().isEquipped)
                RemoveSpawnedItem();
        }

        void RemoveSpawnedItem()
        {
            Debug.Log("Trying to Remove");

            if (spawnedItem != null)
            {
                LevelManager.currentLevel.RemoveDynamic(spawnedItem.GetComponent<Item>());
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
            RemoveSpawnedItem();
        }

        
        // Currently this needs to be ovewritten, looking for a fix
        public override void OnReset()
        {
            OnDeInit();
            OnInit();
        }
        

        Vector3 SpawnLocation()
        {
            return transform.position + spawnOffset;
        }

        public void SpawnItem()
        {
            if (spawnedItem == null)
            {
                spawnedItem = LevelManager.currentLevel.AddDynamic( itemToSpawn, SpawnLocation(), new Quaternion(0, 0, 0, 0));
            }
            else
            {
                LevelManager.currentLevel.AddToDynamic(spawnedItem, SpawnLocation(), new Quaternion(0, 0, 0, 0));
            }
        }

    }
}
