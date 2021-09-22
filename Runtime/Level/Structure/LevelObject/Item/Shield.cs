using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Shield : Item
    {
        public void Update()
        {
            //transform.localEulerAngles = new Vector3(0, 180, 0);
        }

        public override void OnAttach()
        {
            base.OnAttach();
            transform.localPosition = new Vector3(0.002f, 0, 0);
            transform.localEulerAngles = new Vector3(0, 180, 0);
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
        public override void Use()
        {
            base.Use();
            owner.invincible = true;
            Invoke(RaiseShieldEffect,true);
            owner.Invoke(owner.setMovementStatus, false);
        }

        public override void StopUse()
        {
            base.StopUse();
            owner.invincible = false;
            Invoke(LowerShieldEffect,true);
            owner.Invoke(owner.setMovementStatus, true);
        }
    }
}