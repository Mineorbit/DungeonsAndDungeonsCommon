using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityBaseAnimator : LevelObjectBaseAnimator
    {
        public Entity me;
        public Animator animator;

        public EntityAudioController entityAudioController;
        public EntityController entityController;
        
        public float speed;


        public virtual void Update()
        {
            
            if(animator != null)
                animator.SetFloat("Speed", speed);
        }

        public void FixedUpdate()
        {
            if (!me.enabled)
            {
                me.ComputeCurrentSpeed();
                me.UpdateGround();
            }
            speed = me.speed;
            
        }

        public virtual void Hit()
        {
            animator.SetTrigger("Hit");
        }

        public virtual void StopHit()
        {
            animator.SetTrigger("StopHit");
        }

        public virtual void Strike()
        {
            animator.SetTrigger("Strike");
        }

        public virtual void Daze()
        {
            animator.SetBool("Dazed",true);
        }

        public virtual void Death()
        {
            animator.SetTrigger("Death");
        }
        
        public virtual void Undaze()
        {
            animator.SetBool("Dazed",false);
        }
    }
}