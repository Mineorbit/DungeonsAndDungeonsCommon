using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PressurePad : InteractiveLevelObject
    {
        public Hitbox playerStandinghitbox;
        public Collider buildCollider;

        public bool returnToUnpress;

        public float unpressTime = 5f;

        public ButtonBaseAnimator buttonBaseAnimator;

        public bool pressed;
        public TimerManager.Timer unpressTimer;


        public override void OnInit()
        {
            base.OnInit();
            buildCollider.enabled = Level.instantiateType == Level.InstantiateType.Edit;
        }

        public override void OnStartRound()
        {
            base.OnStartRound();
            buildCollider.enabled = false;
            playerStandinghitbox.Attach("Entity");

            playerStandinghitbox.enterEvent.AddListener(x =>
            {
                TimerManager.StopTimer(unpressTimer);
                if (!pressed) Invoke(Activate);
            });
            playerStandinghitbox.exitEvent.AddListener(x => { StartUnpress(); });
        }

        

        public override void Activate()
        {
            if (!pressed)
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

        private void StartUnpress()
        {
            if (returnToUnpress && !TimerManager.isRunning(unpressTimer))
                unpressTimer = TimerManager.StartTimer(unpressTime, () => { Invoke(Deactivate); });
        }


        public override void ResetState()
        {
            base.ResetState();
            bool retToUnp = returnToUnpress;
            returnToUnpress = true;
            Deactivate();
            returnToUnpress = retToUnp;
            // Factor out into parent eventually
            buildCollider.enabled = true;
        }

        public override void Deactivate()
        {
            if (returnToUnpress)
                if (pressed)
                {
                    pressed = false;
                    base.Deactivate();
                    AnimUnpress();
                }
        }
    }
}