using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Trapdoor : InteractiveLevelObject
    {
        public Collider buildCollider;

        public bool returnToUnpress;


        public SwitchingLevelObjectBaseAnimator switchingBaseAnimator;

        public bool pressed;

        public Collider blockingCollider;

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

        

        public override void Activate()
        {
            if (!pressed)
            {
                pressed = true;
                base.Activate();
                blockingCollider.enabled = false;
                AnimOpen();
            }
        }

        public void AnimOpen()
        {
            switchingBaseAnimator.Set(true);
        }

        public void AnimClose()
        {
            switchingBaseAnimator.Set(false);
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
                    blockingCollider.enabled = true;
                    AnimClose();
                }
        }
    }
}