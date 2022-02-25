using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class SpikeBaseAnimator : LevelObjectBaseAnimator
    {
        
        public GameObject[] spikes;
        public void AnimationState(bool b)
        {
            foreach (GameObject g in spikes)
            {
                g.SetActive(b);
            }
        }

        public override void AnimationStateUpdate()
        {
            base.AnimationStateUpdate();
            AnimationState( me.levelObjectNetworkHandler.AnimatorState != 0);
        }
      

        public void SetSpikes(bool open)
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
