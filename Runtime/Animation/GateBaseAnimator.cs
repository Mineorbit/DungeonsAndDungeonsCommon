using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class GateBaseAnimator : LevelObjectBaseAnimator
    {
        public GameObject model;
        private bool open = false;

        public void AnimationState(bool b)
        {
            model.SetActive(b);
        }
        
        
        public void Open()
        {
            if (!open)
            {
                open = true;
                AnimationState(false);
                me.levelObjectNetworkHandler.AnimatorState = 1;
            }
        }

        protected override void AnimationStateUpdate()
        {
            base.AnimationStateUpdate();
            AnimationState( me.levelObjectNetworkHandler.AnimatorState == 0);
        }

        

        public void Close()
        {
            if (open)
            {
                open = false;
                AnimationState(true);
                me.levelObjectNetworkHandler.AnimatorState = 0;
            }
        }
    }
}