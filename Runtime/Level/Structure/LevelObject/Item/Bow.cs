using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Bow : Item
    {
        public override void OnAttach()
        {
            base.OnAttach();
            transform.localPosition = new Vector3(0, 0.002f, 0);
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        // Update is called once per frame
        public override void Use()
        {
            base.Use();
            owner.invincible = true;
            ((PlayerBaseAnimator)owner.baseAnimator).StartAim();
            owner.Invoke(owner.setMovementStatus, false);
        }

        public override void StopUse()
        {
            base.StopUse();
            ((PlayerBaseAnimator)owner.baseAnimator).StopAim();
            owner.Invoke(owner.setMovementStatus, true);
        }
    }
}