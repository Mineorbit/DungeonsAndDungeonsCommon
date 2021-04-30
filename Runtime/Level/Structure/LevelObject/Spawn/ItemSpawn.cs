using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ItemSpawn : Spawn
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
            RemoveSpawnedItem();
        }

        


        void RemoveSpawnedItem(bool physics = true)
        {
            Debug.Log("Trying to Remove");

            if (spawnedItem != null)
            {
                LevelManager.currentLevel.RemoveDynamic(spawnedItem.GetComponent<Item>(), physics: physics);
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
            if (spawnedItem != null && spawnedItem.GetComponent<Item>().isEquipped)
                RemoveSpawnedItem(physics: false);
        }

        Vector3 SpawnLocation()
        {
            return transform.position + spawnOffset;
        }

        Quaternion SpawnRotation()
        {
            return transform.rotation;
        }

        public void SpawnItem()
        {
            if (spawnedItem == null)
            {
                spawnedItem = LevelManager.currentLevel.AddDynamic( itemToSpawn, SpawnLocation(), SpawnRotation());
            }
            else
            {
                LevelManager.currentLevel.AddToDynamic(spawnedItem, SpawnLocation(), SpawnRotation());
            }
        }

    }
}
