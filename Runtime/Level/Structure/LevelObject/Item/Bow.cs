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
        }

        public void StartAimEffect()
        {
            ((PlayerBaseAnimator)owner.baseAnimator).StartAim();
        }

        public void StopAimEffect()
        {
            ((PlayerBaseAnimator)owner.baseAnimator).StopAim();
        }
        
        // Update is called once per frame
        public override void Use()
        {
            base.Use();
            ((Player)owner).aiming = true;
            owner.setMovementStatus( false);
            Invoke(StartAimEffect);
            Invoke(CreateArrow,false,true);
        }

        void ResetHolding()
        {
            
        }
        public override void StopUse()
        {
            base.StopUse();
            Invoke("ResetHolding",1f);
            ((Player)owner).aiming = false;
            Invoke(StopAimEffect);
            owner.setMovementStatus(true);
            
            //GameConsole.Log($"Shooting towards {owner.GetAimRotation().eulerAngles} call");
            Invoke(Shoot,false,true);
        }

        public void Shoot()
        {
            if(currentArrow!=null)
            {
                GameConsole.Log($"Shooting towards {owner.aimRotation.eulerAngles}");
                currentArrow.Shoot(owner.aimRotation);
                currentArrow = null;
            }
        }

        public Arrow currentArrow;
        
        public LevelObjectData arrow;
        public void CreateArrow()
        {
            if(currentArrow == null)
            {
                GameConsole.Log("Creating Arrow");
                GameObject arrowObject = LevelManager.currentLevel.AddDynamic(arrow,transform.position, Quaternion.LookRotation(Vector3.forward), new Util.Optional<int>());
                currentArrow = arrowObject.GetComponent<Arrow>();
                currentArrow.shootingBow = this;
            }
        }
    }
}