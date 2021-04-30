using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class ButtonBaseAnimator : BaseAnimator
    {


        public Transform buttonHead;


        IEnumerator PressAnimation()
        {
                float time = 0.25f;
                float elapsedTime = 0;

                buttonHead.localScale = new Vector3(70, 70, 70);

                while (elapsedTime < time)
                {
                    float t = (elapsedTime / time);
                    float inert = 10 * Mathf.Sin(t * Mathf.PI);
                    buttonHead.localScale = new Vector3(70 + inert, 70 + inert, (Mathf.Pow((1 - t), 3)) * 40 + 30);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
        }

        IEnumerator UnpressAnimation()
        {
            
                float time = 0.25f;
                float elapsedTime = 0;

                buttonHead.localScale = new Vector3(70, 70, 30);

                while (elapsedTime < time)
                {
                    float t = (elapsedTime / time);
                    float inert = 10 * Mathf.Sin((1 - t) * Mathf.PI);
                    buttonHead.localScale = new Vector3(70 + inert, 70 + inert, (Mathf.Pow((1 - (1 - t)), 3)) * 40 + 30);
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
