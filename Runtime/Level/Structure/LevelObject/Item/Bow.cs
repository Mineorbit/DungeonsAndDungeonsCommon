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
            transform.localEulerAngles = new Vector3(0, 110, 85);
            ((PlayerBaseAnimator)owner.baseAnimator).StartAim();
            ((Player)owner).aiming = true;
            owner.Invoke(owner.setMovementStatus, false);
        }

        void ResetHolding()
        {
            
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        public override void StopUse()
        {
            base.StopUse();
            Invoke("ResetHolding",1f);
            ((PlayerBaseAnimator)owner.baseAnimator).StopAim();
            ((Player)owner).aiming = false;
            owner.Invoke(owner.setMovementStatus, true);
            Invoke(Shoot,true);
        }

        public LevelObjectData arrow;
        public void Shoot()
        {
            GameObject arrowObject = LevelManager.currentLevel.AddDynamic(arrow,transform.position, Quaternion.LookRotation(Camera.main.transform.right), new Util.Optional<int>());
            arrowObject.GetComponent<Arrow>().shootingBow = this;
        }
    }
}