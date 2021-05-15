using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Item : NetworkLevelObject
    {

        public Rigidbody rigidBody;

        public float useTime = 0.25f;
        public Entity owner;

        public bool isEquipped;

        public UnityEvent onUseEvent = new UnityEvent();
        public UnityEvent onStopUseEvent = new UnityEvent();

        public virtual void OnAttach()
        {
            isEquipped = true;
            owner = GetComponentInParent<Entity>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            GetComponent<Collider>().enabled = false;
        }

        public virtual void OnDettach()
        {
            isEquipped = false;
            LevelManager.currentLevel.AddToDynamic(this.gameObject);


            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            GetComponent<Collider>().enabled = true;
        }

        public virtual void Use()
        {
            onUseEvent.Invoke();
        }

        public virtual void StopUse()
        {
            onStopUseEvent.Invoke();

        }
    }
}
