using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class GoalAudioController : AudioController
    {
        public void PlayAmbient()
        {
            Play(0);
        }
        public float PlayComplete()
        {
            AudioSource ambientSource = GetAudioSource(0,0);
            float queueNewSoundTime = ambientSource.clip.length - ambientSource.time;
            CrossFade(0,1,queueNewSoundTime);
            return queueNewSoundTime;
        }
        
    }
}