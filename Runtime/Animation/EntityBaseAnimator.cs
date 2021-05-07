using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityBaseAnimator : BaseAnimator
    {
        public Entity me;
        public Animator animator;

        public float speed;

        // Update is called once per frame
        public virtual void Update()
        {
            base.Update();
        }



        public void Hit()
        {
            animator.SetTrigger("Hit");
        }


        public void Strike()
        {
            animator.SetTrigger("Strike");
        }
    }
}
