using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common.Merge;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class SwitchingLevelObjectBaseAnimator : LevelObjectBaseAnimator
    {
        public Animator animator;
        public bool defaultState = false;
        public void AnimationState(bool b)
        {
            GameConsole.Log("Changing state to "+b);
            animator.SetBool("on",defaultState ^ b);
        }

        protected override void AnimationStateUpdate()
        {
            base.AnimationStateUpdate();
            AnimationState( me.levelObjectNetworkHandler.AnimatorState == 0);
        }

        
        public void Set(bool open)
        {
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
