using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityBaseAnimator : BaseAnimator
    {
        public Entity me;
        public Animator animator;

        public EntityController entityController;
        
        public float speed;


        public virtual void Update()
        {
            if(animator != null)
                animator.SetFloat("Speed", speed);
        }

        public virtual void Hit()
        {
            GameConsole.Log("Hit called");
            animator.SetTrigger("Hit");
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
        
        public void Undaze()
        {
            animator.SetBool("Dazed",false);
        }
    }
}