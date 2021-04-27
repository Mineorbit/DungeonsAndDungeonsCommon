using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Item : LevelObject
    {

        public float useTime = 0.25f;
        public Entity owner;

        public bool isEquipped;

        public virtual void OnAttach()
        {
            isEquipped = true;
            owner = GetComponentInParent<Entity>();
        }

        public virtual void OnDettach()
        {
            isEquipped = false;
            LevelManager.currentLevel.AddToDynamic(this.gameObject);
        }

        public virtual void Use()
        {

        }

        public virtual void StopUse()
        {

        }
    }
}
