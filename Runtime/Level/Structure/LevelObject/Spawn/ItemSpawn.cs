using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ItemSpawn : Spawn
    {
        public LevelObjectData itemToSpawn;

        private GameObject model;

        public Vector3 spawnOffset;

        private readonly int maxSpawnCount = 1;

        private int spawnCount = 1;

        private GameObject spawnedItem;

        //Change to on remove

        public void OnDestroy()
        {
            RemoveSpawnedItem();
        }

        public override void OnStartRound()
        {
            Setup();
            //spawnedItem.GetComponent<Item>().Spawn(SpawnLocation(),SpawnRotation(),true);
        }

        public override void OnEndRound()
        {
            SetCollider();
        }

        

        private void Setup()
        {
            spawnCount = maxSpawnCount;
            SpawnItem();
            model = transform.Find("Model").gameObject;
            SetCollider();
            model.SetActive(false);
        }


        private void RemoveSpawnedItem(bool physics = true)
        {
            if (spawnedItem != null) LevelManager.currentLevel.RemoveDynamic(spawnedItem.GetComponent<Item>(), physics);
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
                RemoveSpawnedItem(false);
        }

        private Vector3 SpawnLocation()
        {
            return transform.position + spawnOffset;
        }

        private Quaternion SpawnRotation()
        {
            return transform.rotation;
        }

        public void SpawnItem()
        {
            if (Level.instantiateType != Level.InstantiateType.Online)
            {
                if (spawnedItem == null)
                    spawnedItem = LevelManager.currentLevel.AddDynamic(itemToSpawn, SpawnLocation(), SpawnRotation(),null);
                else
                    spawnedItem.GetComponent<Item>().Spawn(SpawnLocation(), SpawnRotation(),true);
            }
        }
    }
}