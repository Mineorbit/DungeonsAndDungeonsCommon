using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelNavGenerator : MonoBehaviour
    {
        public NavMeshSurface navMeshSurface;
        void Start()
        {
            if (navMeshSurface == null) navMeshSurface = GetComponent<NavMeshSurface>();

        }

        public void UpdateNavMesh()
        {
            Debug.Log("Building");
            List<LevelObject> dynObjects = LevelManager.currentLevel.GetAllDynamicLevelObjects(inactive: false);
            foreach (LevelObject o in dynObjects)
            {
                    o.gameObject.SetActive(false);
            }

            if (navMeshSurface == null) navMeshSurface = GetComponent<NavMeshSurface>();


            navMeshSurface.BuildNavMesh();

            Debug.Log("Built surface");

            foreach (LevelObject o in dynObjects)
            {
                o.gameObject.SetActive(true);
            }
        }
    }
}
