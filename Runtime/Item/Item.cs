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
            Debug.Log(gameObject.name+" was attached");
            owner = GetComponentInParent<Player>().gameObject;
        }

        public virtual void OnDettach()
        {

        }

        public virtual void Use()
        {

        }

        public virtual void StopUse()
        {

        }
    }
}
