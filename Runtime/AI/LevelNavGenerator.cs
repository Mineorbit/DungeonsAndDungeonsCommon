using System.Collections;
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

        public void UpdateNavMesh()
        {
            Debug.Log("Building");
            Transform p = LevelManager.currentLevel.GetAllDynamicObjects();
            foreach (Transform t in p)
            {
                if (t != null)
                    t.gameObject.SetActive(false);
            }

            if (navMeshSurface == null) navMeshSurface = GetComponent<NavMeshSurface>();
            navMeshSurface.BuildNavMesh();

            foreach (Transform t in p)
            {
                if (t != null)
                    t.gameObject.SetActive(true);
            }
        }
    }
}
