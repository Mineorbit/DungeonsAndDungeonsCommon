using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common.Merge;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class SwitchingLevelObjectBaseAnimator : LevelObjectBaseAnimator
    {
        public Animator animator;
        private bool state = false;
        public void AnimationState(bool b)
        {
            animator.SetBool("on",b);
        }

        protected override void AnimationStateUpdate()
        {
            base.AnimationStateUpdate();
            AnimationState( me.levelObjectNetworkHandler.AnimatorState == 0);
        }

        public void Switch()
        {
            Set(!state);
        }
        
        public void Set(bool open)
        {
            state = open;
            AnimationState(open);
            if (open)
            {
                me.levelObjectNetworkHandler.AnimatorState = 1;
            }
            else
            {
                me.levelObjectNetworkHandler.AnimatorState = 0;
            }
        }
    }
}
