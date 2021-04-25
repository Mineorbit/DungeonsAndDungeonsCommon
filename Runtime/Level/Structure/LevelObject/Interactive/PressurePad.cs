using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class PressurePad : InteractiveLevelObject
    {
        public Hitbox playerStandinghitbox;
        public Transform buttonHead;
        public Collider buildCollider;

        bool pressed = false;
        IEnumerator unpressTimer;
        public override void OnInit()
        {
            base.OnInit();
            playerStandinghitbox.Attach("Player");
            playerStandinghitbox.enterEvent.AddListener((x)=> { if (unpressTimer != null) { StopCoroutine(unpressTimer); unpressTimer = null; } StartCoroutine("Press");});
            playerStandinghitbox.exitEvent.AddListener((x) => { unpressTimer = TimerUnpress(); StartCoroutine(unpressTimer); });
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            buildCollider.enabled = false;
        }

        public override void OnEndRound()
        {
            base.OnEndRound();
            buildCollider.enabled = true;
        }

        IEnumerator Press()
        {
            if(!pressed)
            {
            pressed = true;
            float time = 0.25f;
            float elapsedTime = 0;

            buttonHead.localScale = new Vector3(70,70,70);

            while (elapsedTime < time)
            {
                    float t = (elapsedTime / time);
                    float inert = 10*Mathf.Sin(t*Mathf.PI);
                buttonHead.localScale = new Vector3(70+inert, 70+inert,  (Mathf.Pow((1-t),3))*40+30);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Activate();
            }
        }

        IEnumerator TimerUnpress()
        {
            yield return new WaitForSeconds(5);
            StartCoroutine("Unpress");
        }

        IEnumerator Unpress()
        {
            if (pressed)
            {
                float time = 0.25f;
                float elapsedTime = 0;

                buttonHead.localScale = new Vector3(70, 70, 30);

                while (elapsedTime < time)
                {
                    float t = (elapsedTime / time);
                    float inert = 10 * Mathf.Sin((1-t) * Mathf.PI);
                    buttonHead.localScale = new Vector3(70 + inert, 70 + inert, (Mathf.Pow((1 - (1-t)), 3)) * 40 + 30);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                Deactivate();
                pressed = false;
            }
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
