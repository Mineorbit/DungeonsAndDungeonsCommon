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

            playerStandinghitbox.enterEvent.AddListener((x) => { TimerManager.StopTimer(unpressTimer); if(!pressed) PressButton(); });
            playerStandinghitbox.exitEvent.AddListener((x) => { StartUnpress(); });


        }

        public override void OnEndRound()
        {
            base.OnEndRound();
            UnpressButton();
            buildCollider.enabled = true;
        }

        void PressButton()
        {
            if(!pressed)
            {
                pressed = true;
                Activate();
                Invoke(AnimPress);
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
                unpressTimer = TimerManager.StartTimer(unpressTime, ()=> { UnpressButton(); });
        }
        

        void UnpressButton()
        {
            if(returnToUnpress)
            { 
                if(pressed)
                {
                    pressed = false;
                    Deactivate();
                    Invoke(AnimUnpress);
                }
            }
        }

        
        // Update is called once per frame
        void Update()
        {

        }
    }
}
