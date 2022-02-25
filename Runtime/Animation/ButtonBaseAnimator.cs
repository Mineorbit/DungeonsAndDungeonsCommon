using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ButtonBaseAnimator : LevelObjectBaseAnimator
    {
        public Transform buttonHead;


        public UnityEvent buttonPressEvent = new UnityEvent();
        public UnityEvent buttonUnpressEvent = new UnityEvent();

        public void AnimationState(float t,bool b)
        {
            var inert = b? 10 * Mathf.Sin(t * Mathf.PI) :  10 * Mathf.Sin((1 - t) * Mathf.PI);
            buttonHead.localScale = new Vector3(70 + inert, 70 + inert, Mathf.Pow(1 - t, 3) * 40 + 30);
        }

        public override void AnimationStateUpdate()
        {
            base.AnimationStateUpdate();
            AnimationState(0, me.levelObjectNetworkHandler.AnimatorState != 0);
        }


        private IEnumerator PressAnimation()
        {
            buttonPressEvent.Invoke();
            var time = 0.25f;
            float elapsedTime = 0;
            AnimationState(0,false);
            while (elapsedTime < time)
            {
                var t = elapsedTime / time;
                AnimationState(t,true);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            me.levelObjectNetworkHandler.AnimatorState = 0;
        }

        private IEnumerator UnpressAnimation()
        {
            buttonUnpressEvent.Invoke();
            var time = 0.25f;
            float elapsedTime = 0;

            AnimationState(0,true);
            
            while (elapsedTime < time)
            {
                var t = elapsedTime / time;
                AnimationState(t,false);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            me.levelObjectNetworkHandler.AnimatorState = 1;
        }

        public void Press()
        {
            StartCoroutine("PressAnimation");
        }

        public void Unpress()
        {
            StartCoroutine("UnpressAnimation");
        }
    }
}