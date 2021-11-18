using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ButtonBaseAnimator : BaseAnimator
    {
        public Transform buttonHead;


        public UnityEvent buttonPressEvent = new UnityEvent();
        public UnityEvent buttonUnpressEvent = new UnityEvent();
        
        private IEnumerator PressAnimation()
        {
            buttonPressEvent.Invoke();
            var time = 0.25f;
            float elapsedTime = 0;

            buttonHead.localScale = new Vector3(70, 70, 70);

            while (elapsedTime < time)
            {
                var t = elapsedTime / time;
                var inert = 10 * Mathf.Sin(t * Mathf.PI);
                buttonHead.localScale = new Vector3(70 + inert, 70 + inert, Mathf.Pow(1 - t, 3) * 40 + 30);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator UnpressAnimation()
        {
            buttonUnpressEvent.Invoke();
            var time = 0.25f;
            float elapsedTime = 0;

            buttonHead.localScale = new Vector3(70, 70, 30);

            while (elapsedTime < time)
            {
                var t = elapsedTime / time;
                var inert = 10 * Mathf.Sin((1 - t) * Mathf.PI);
                buttonHead.localScale = new Vector3(70 + inert, 70 + inert, Mathf.Pow(1 - (1 - t), 3) * 40 + 30);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
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