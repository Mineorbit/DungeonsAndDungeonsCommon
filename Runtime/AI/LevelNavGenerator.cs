﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelNavGenerator : MonoBehaviour
    {
        public static NavMeshSurface navMeshSurface;
        void Start()
        {
            if (navMeshSurface == null) navMeshSurface = GetComponent<NavMeshSurface>();

        }

        public static void UpdateNavMesh()
        {
            Debug.Log("Building");
            GameObject[] enemies = Level.GetAllEnemies();
            foreach (GameObject e in enemies)
            {
                if (e != null)
                    e.SetActive(false);
            }

            navMeshSurface.BuildNavMesh();

            foreach (GameObject e in enemies)
            {
                if (e != null)
                    e.SetActive(true);
            }
        }
    }
}
