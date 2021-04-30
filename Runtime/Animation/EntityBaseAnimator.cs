using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityBaseAnimator : BaseAnimator
    {
        public Entity me;
        public Animator animator;
        

        // Update is called once per frame
        void Update()
        {

        }
        public void Strike()
        {
            animator.SetTrigger("Strike");
        }
    }
}
