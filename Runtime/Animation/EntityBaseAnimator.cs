using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityBaseAnimator : BaseAnimator
    {
        public Entity me;
        public Animator animator;

        public float speed;

        
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