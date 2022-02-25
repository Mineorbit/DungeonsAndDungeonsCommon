using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectBaseAnimator : BaseAnimator
    {
        public NetworkLevelObject me;
        public virtual void Start()
        {
            me.levelObjectNetworkHandler.valueChangedEvent.AddListener(AnimationStateUpdate);
        }

        public virtual void AnimationStateUpdate()
        {
            
        }
    }
}