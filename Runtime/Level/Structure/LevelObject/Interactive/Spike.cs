using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Spike : InteractiveLevelObject
    {
        private int spikeDamage = 50;
        // AUTOMATICALLY CONNECT TO ALL NEIGHBORING ON PLAY

        public SwitchingLevelObjectBaseAnimator spikeBaseAnimator;
        public Hitbox Hitbox;
        public Collider buildCollider;
        
        
        public override void OnInit()
        {
            base.OnInit();
            buildCollider.enabled = Level.instantiateType == Level.InstantiateType.Edit;
        }
        
        public override void OnStartRound()
        {
            base.OnStartRound();
            buildCollider.enabled = false;
            Hitbox.Attach("Entity");
            Hitbox.enterEvent.AddListener((x)=>TryDamage(x));
            Deactivate();
        }

        private void TryDamage(GameObject g)
        {
            Vector3 dir = g.transform.position - transform.position;
            dir.Normalize();
            GameConsole.Log($"Direction: {Vector3.Dot(transform.up,dir)}");
            if(Vector3.Dot(transform.up,dir)> 0.7f)
            {
                var c = g.GetComponentInParent<Entity>(true);
                if (c != null)
                {
                    c.Hit( this, spikeDamage);
                }
            }
        }
        

        public override void OnActivated()
        {
            spikeBaseAnimator.Set(false);
            Hitbox.Deactivate();
        }

        public override void OnDeactivated()
        { 
            spikeBaseAnimator.Set(true);
            Hitbox.Activate();
        }
        
        
        public override void ResetState()
        {
            base.ResetState();
            buildCollider.enabled = true;
            Deactivate();
        }
    }
}