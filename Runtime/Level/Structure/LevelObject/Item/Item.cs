using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Item : Entity
    {

        public float useTime = 0.25f;
        public Entity owner;

        public bool isEquipped;

        public UnityEvent onUseEvent = new UnityEvent();
        public UnityEvent onStopUseEvent = new UnityEvent();
        public UnityEvent onAttachEvent = new UnityEvent();
        public UnityEvent onDettachEvent = new UnityEvent();
        public UnityEvent onCollideEvent = new UnityEvent();

        
        
        public ItemHandle itemHandle;
        
        public enum Side
        {
            Left,
            Right
        }

        public Transform attachmentPoint;

        public void SetupAttachmentPoint()
        {
            if (attachmentPoint == null)
            {
                GameObject attPoint = new GameObject("AttachmentPoint");
                attPoint.transform.SetParent(this.transform);
                attPoint.transform.localPosition = Vector3.zero;
                attachmentPoint = attPoint.transform;
            }
        }

        public Vector3 GetAttachmentPoint()
        {
            
            if(attachmentPoint != null)
            {
                return Vector3.zero - attachmentPoint.localPosition;
            }
            else
            {
                GameConsole.Log("No attachment point");
                return Vector3.zero;
            }
        }
        
        public Side equipSide;
        public virtual void OnAttach()
        {
            isEquipped = true;
            owner = transform.parent.GetComponentInParent<Entity>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            GetComponent<Collider>().enabled = false;
            transform.localRotation = Quaternion.identity;
            onAttachEvent.Invoke();
        }

        public virtual void SetAttachmentPosition()
        {
            
        }

        public virtual void OnDettach()
        {
            isEquipped = false;
            LevelManager.currentLevel.AddToDynamic(gameObject);
    

            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            GetComponent<Collider>().enabled = true;
            onDettachEvent.Invoke();
        }

        public override void OnInit()
        {
            base.OnInit();
            this.gameObject.tag = "Item";
            //SetupAttachmentPoint();
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