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

        public override void Use()
        {
            base.Use();
            owner.invincible = true;
            ((PlayerBaseAnimator)owner.baseAnimator).RaiseShield();
            owner.Invoke(owner.setMovementStatus, false);
        }

        public override void StopUse()
        {
            base.StopUse();
            owner.invincible = false;
            owner.Invoke(owner.setMovementStatus, true);
        }
    }
}