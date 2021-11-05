using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Item : Entity
    {
        public Rigidbody rigidBody;

        public float useTime = 0.25f;
        public Entity owner;

        public bool isEquipped;

        public UnityEvent onUseEvent = new UnityEvent();
        public UnityEvent onStopUseEvent = new UnityEvent();
        public UnityEvent onAttachEvent = new UnityEvent();
        public UnityEvent onDettachEvent = new UnityEvent();

        public ItemHandle itemHandle;
        
        public enum Side
        {
            Left,
            Right
        }
        

        public Side equipSide;
        public virtual void OnAttach()
        {
            isEquipped = true;
            owner = transform.parent.GetComponentInParent<Entity>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            GetComponent<Collider>().enabled = false;
            onAttachEvent.Invoke();
        }

        public virtual void SetAttachmentPosition()
        {
            
        }

        public virtual void OnDettach()
        {
            isEquipped = false;
            LevelManager.currentLevel.AddToDynamic(gameObject);
    

            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            GetComponent<Collider>().enabled = true;
            onDettachEvent.Invoke();
        }

        public override void OnInit()
        {
            base.OnInit();
            this.gameObject.tag = "Item";
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //GameConsole.Log("Removing this");
            //Debug.Break();
        }
        
        public virtual void Use()
        {
            onUseEvent.Invoke();
        }

        public virtual void StopUse()
        {
            onStopUseEvent.Invoke();
        }
        //QUICK FIX
        public virtual void StopUse(Quaternion arg)
        {
            onStopUseEvent.Invoke();
        }
    }
}