using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Logic : MonoBehaviour
    {
        public GameObject[] created;
        public bool running;
        public int sceneIndex;

        public static Logic current;
        
        public static float time;

        public void FixedUpdate()
        {
            if (running)
            {
                time += Time.fixedDeltaTime;
            }
        }

        public GameObject[] FetchAllinScene()
        {
            var objs = new List<GameObject>();
            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
                if (obj.scene.buildIndex == sceneIndex)
                    objs.Add(obj);
            if (objs.Count == 0) return new GameObject[0];
            return objs.ToArray();
        }

        public void SpawnAll()
        {
            created = FetchAllinScene();
            foreach (var g in created) g.SetActive(true);
        }

        public void DespawnAll()
        {
            created = FetchAllinScene();
            foreach (var g in created) g.SetActive(false);
        }

        public virtual void PlayerDeath()
        {
            
        }

        public virtual void RespawnPlayer(int localId)
        {
            PlayerManager.playerManager.SpawnPlayer(localId,PlayerManager.playerManager.GetSpawnLocation(localId)+Vector3.up*0.125f);
        }
        
        public virtual void Init()
        {
        }

        public virtual void Start()
        {
            running = true;
            time = 0;
        }

        public virtual void Stop()
        {
            running = false;
        }

        public virtual void DeInit()
        {
        }
    }
}