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

        private float returnTime = 8f;
        public float PlayComplete()
        {
            AudioSource ambientSource = GetAudioSource(0,0);
            float queueNewSoundTime = ambientSource.clip.length - ambientSource.time;
            StartCoroutine(returnToRegular(queueNewSoundTime));
            return queueNewSoundTime+returnTime;
        }

        IEnumerator returnToRegular(float transferTime)
        {;
            yield return new WaitForSeconds(transferTime);
            CrossFade(0,1,0.5f);
            yield return new WaitForSeconds(returnTime);
                CrossFade(1,0,2);
        }
    }
}