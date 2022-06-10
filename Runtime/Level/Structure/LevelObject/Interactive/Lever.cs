using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Lever : InteractableLevelObject
    {
        public Collider buildCollider;

        public bool returnToUnpress;

        public float unpressTime = 5f;

        public LeverBaseAnimator leverBaseAnimator;

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
        }

        private bool switched = false;

        public override void Interact()
        {
            base.Interact();
            if (!switched)
            {
                Activate();
                switched = true;
            }
            else
            {
                Deactivate();
                switched = false;
            }
            
        }
        
        public override void Activate()
        {
                base.Activate();
                Invoke(AnimSwitch);
        }
        
        public override void Deactivate()
        {
            base.Deactivate();
            Invoke(AnimSwitch);
        }

        public void AnimSwitch()
        {
            leverBaseAnimator.Switch();
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

    }
}