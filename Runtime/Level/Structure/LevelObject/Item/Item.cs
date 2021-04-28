using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Item : LevelObject
    {

        public float useTime = 0.25f;
        public Entity owner;

        public bool isEquipped;

        public UnityEvent onUseEvent = new UnityEvent();

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
            onUseEvent.Invoke();
        }

        public virtual void StopUse()
        {

        }
    }
}
