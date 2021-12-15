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

        public void Hit()
        {
            animator.SetTrigger("Hit");
        }


        public void Strike()
        {
            animator.SetTrigger("Strike");
        }

        public void Daze()
        {
            animator.SetBool("Dazed",true);
        }
        
        public void Undaze()
        {
            animator.SetBool("Dazed",false);
        }
    }
}