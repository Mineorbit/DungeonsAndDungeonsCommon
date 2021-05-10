using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class EntityController : MonoBehaviour
    {

        public Vector3 controllerPosition;
        public virtual void Start()
        {
            controllerPosition = transform.position;
        }

        public void OnSpawn(Vector3 location)
        {
            controllerPosition = location;
        }

    }
}
