using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class PressurePad : InteractiveLevelObject
    {
        public Hitbox playerStandinghitbox;
        public Collider buildCollider;

        public bool returnToUnpress;

        bool pressed = false;

        public float unpressTime = 5f;
        TimerManager.Timer unpressTimer;

        public ButtonBaseAnimator buttonBaseAnimator;

        public override void OnInit()
        {
            base.OnInit();
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            buildCollider.enabled = false;
            playerStandinghitbox.Attach("Player");

            playerStandinghitbox.enterEvent.AddListener((x) => { TimerManager.StopTimer(unpressTimer); if(!pressed) Invoke(Activate); });
            playerStandinghitbox.exitEvent.AddListener((x) => { StartUnpress(); });


        }

        public override void OnEndRound()
        {
            base.OnEndRound();
            Invoke(Deactivate);
            buildCollider.enabled = true;
        }

        public override void Activate()
        {
            if(!pressed)
            {
                pressed = true;
                base.Activate();
                AnimPress();
            }
        }

        public void AnimPress()
        {
            buttonBaseAnimator.Press();
        }

        public void AnimUnpress()
        {
            buttonBaseAnimator.Unpress();
        }

        void StartUnpress()
        {
            if (returnToUnpress && !TimerManager.isRunning(unpressTimer))
                unpressTimer = TimerManager.StartTimer(unpressTime, ()=> { Invoke(Deactivate); });
        }
        

        public override void Deactivate()
        {
            if(returnToUnpress)
            { 
                if(pressed)
                {
                    pressed = false;
                    base.Deactivate();
                    AnimUnpress();
                }
            }
        }

        
        // Update is called once per frame
        void Update()
        {

        }
    }
}
