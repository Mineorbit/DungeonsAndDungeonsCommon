using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectBaseAnimator : BaseAnimator
    {
        public NetworkLevelObject me;
        public static List<LevelObjectBaseAnimator> activeAnimators = new List<LevelObjectBaseAnimator>();
        
        
        public override void Start()
        {
            base.Start();
            activeAnimators.Add(this);
        }

        public void OnDestroy()
        {
            activeAnimators.Remove(this);
        }

        private int lastState = 0;

        public void UpdateAnimator()
        {
            if(lastState != me.levelObjectNetworkHandler.AnimatorState)
            {
                GameConsole.Log($"Animator {this} State changing to {me.levelObjectNetworkHandler.AnimatorState}");
                AnimationStateUpdate();
                lastState = me.levelObjectNetworkHandler.AnimatorState;
            }
        }


        protected virtual void AnimationStateUpdate()
        {
            
        }

        public static void NetTick()
        {
            if (Level.instantiateType == Level.InstantiateType.Online)
            {
                foreach (LevelObjectBaseAnimator o in activeAnimators)
                {
                    o.UpdateAnimator();
                }
            }
        }
    }
}