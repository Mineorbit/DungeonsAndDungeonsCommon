using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Shield : Item
    {
        public void Update()
        {
            //transform.localEulerAngles = new Vector3(0, 180, 0);
        }
        
        Vector3 holdOffset = new Vector3(0.002f, 0, 0);
        private Vector3 backOffset = new Vector3(-0.005f, 0, 0);
        Vector3 holdAngles = new Vector3(0, 180, 0);
        private Vector3 backAngles = new Vector3(-90,0,0);

        public override void OnAttach()
        {
            base.OnAttach();
            SetAttachmentPosition();
        }
        public override void SetAttachmentPosition()
        {
            transform.localPosition = itemHandle.Back ? backOffset : holdOffset;
            transform.localEulerAngles = itemHandle.Back ? backAngles : holdAngles;
        }
        
        // EFFECT NOT YET MIRRORED
        public void RaiseShieldEffect()
        {
            ((PlayerBaseAnimator)owner.baseAnimator).RaiseShield();
        }
        public void LowerShieldEffect()
        {
            ((PlayerBaseAnimator)owner.baseAnimator).LowerShield();
        }

        
        public bool raised = false;

        public float blockAngle = 50;

        private bool doBlockEffect = false;
        private float blockEffectTime = 1f;
        public void BlockAttackEffect()
        {
            if (!doBlockEffect)
            {
                doBlockEffect = true;
                ((PlayerBaseAnimator)owner.baseAnimator).BlockShield();
                Invoke("resetBlockEffect",blockEffectTime);
            }
        }

        public void resetBlockEffect()
        {
            doBlockEffect = false;
        }
        
        public override void Use()
        {
            base.Use();
            raised = true;
            Invoke(RaiseShieldEffect,true);
            owner.Invoke(owner.setMovementStatus, false);
        }

        public override void StopUse()
        {
            base.StopUse();
            raised = false;
            Invoke(LowerShieldEffect,true);
            owner.Invoke(owner.setMovementStatus, true);
        }
    }
}