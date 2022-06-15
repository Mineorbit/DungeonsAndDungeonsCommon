using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Lever : InteractableLevelObject
    {
        public Collider buildCollider;

        public bool returnToUnpress;

        public float unpressTime = 5f;

        public SwitchingLevelObjectBaseAnimator leverBaseAnimator;

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
            Deactivate();
        }

        public override void Interact()
        {
            base.Interact();
            if (!activated)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
            
        }
        
        public override void OnActivated()
        { 
            leverBaseAnimator.Set(true);
        }
        
        public override void OnDeactivated()
        {
            leverBaseAnimator.Set(false);
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