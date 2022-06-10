using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LeverBaseAnimator : LevelObjectBaseAnimator
    {
        public Animator animator;
        public void Switch()
        {
            animator.SetTrigger("Switch");
        }
    }
}