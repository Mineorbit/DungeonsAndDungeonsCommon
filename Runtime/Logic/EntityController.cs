using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class EntityController : MonoBehaviour
    {
        bool dirty;
        Vector3 positionUpdate;

        public Vector3 controllerPosition;
        public virtual void Start()
        {
            controllerPosition = transform.position;
        }

        public void OnSpawn(Vector3 location)
        {
            controllerPosition = location;
        }

        public virtual void Update()
        {
            if(dirty)
            {
                controllerPosition = positionUpdate;
                dirty = false;
            }
        }

        public void UpdatePosition(Vector3 position)
        {
            dirty = true;
            positionUpdate = position;
        }
    }
}
