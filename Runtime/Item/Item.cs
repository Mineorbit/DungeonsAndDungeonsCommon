using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Item : LevelObject
    {

        public float useTime = 0.25f;
        public GameObject owner;

        public virtual void OnAttach()
        {
            owner = GetComponentInParent<Player>().gameObject;
        }
        public virtual void OnDettach()
        {

        }

        public void OnDestroy()
        {
            Debug.Log("Removed Item");
        }

        public virtual void Use()
        {

        }

        public virtual void StopUse()
        {

        }
    }
}
