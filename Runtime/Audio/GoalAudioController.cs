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

        public void Level(int numberOfPlayers)
        {
            Fade(0,2f,((float) numberOfPlayers)/4f);
            // ALSO CHANGE SPATIAL PART IN FUTURE TO LOWER VALUE
        }
        public float PlayComplete()
        {
            AudioSource ambientSource = GetAudioSource(0,0);
            AudioSource goalSource = GetAudioSource(1, 0);
            float queueNewSoundTime = ambientSource.clip.length - ambientSource.time;
            StartCoroutine(returnToRegular(queueNewSoundTime,goalSource.clip.length));
            return queueNewSoundTime+goalSource.clip.length;
        }

        IEnumerator returnToRegular(float transferTime,float returnTime)
        {;
            yield return new WaitForSeconds(transferTime);
            CrossFade(0,1,0.5f);
            yield return new WaitForSeconds(returnTime);
                CrossFade(1,0,2);
        }
    }
}