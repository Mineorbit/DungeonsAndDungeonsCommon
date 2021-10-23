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
            transform.localEulerAngles = new Vector3(0, 110, 85);
            ((PlayerBaseAnimator)owner.baseAnimator).StartAim();
            ((Player)owner).aiming = true;
            owner.Invoke(owner.setMovementStatus, false);
            Invoke(CreateArrow,false);
            //CreateArrow();
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
            Invoke<UnityEngine.Quaternion>(Shoot,owner.GetAimRotation(),true);
        }

        public void Shoot(Quaternion aimRotation)
        {
            currentArrow.Shoot(aimRotation);
            currentArrow = null;
        }

        public Arrow currentArrow;
        
        public LevelObjectData arrow;
        public void CreateArrow()
        {
            if(currentArrow == null)
            {
                GameObject arrowObject = LevelManager.currentLevel.AddDynamic(arrow,transform.position, Quaternion.LookRotation(Vector3.forward), new Util.Optional<int>());
                currentArrow = arrowObject.GetComponent<Arrow>();
                currentArrow.shootingBow = this;
            }
        }
    }
}