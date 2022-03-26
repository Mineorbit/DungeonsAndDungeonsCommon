using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class DestructibleLevelObject : NetworkLevelObject
    {
        public bool destroyed = false;
        public Collider collider;
        public virtual void Destroy(Vector3 transformPosition)
        {
            if (!destroyed)
            {
                destroyed = true;
                collider.enabled = false;
                Invoke(DestroyEffect,transformPosition);
            }
        }
        public virtual void DestroyEffect(Vector3 origin)
        {
            DestroyState(true);
        }
        public virtual void DestroyEffect()
        {
            DestroyState(true);
        }
        public virtual void DestroyState(bool d)
        {
            
        }
        
        public override void OnInit()
        {
            base.OnInit();
            destroyed = false;
            collider.enabled = true;
            DestroyState(false);
            
            
        }

        

        public override void OnStartRound()
        {
            base.OnStartRound();
            destroyed = false;
            collider.enabled = true;
            DestroyState(false);
        }
        
    }
}