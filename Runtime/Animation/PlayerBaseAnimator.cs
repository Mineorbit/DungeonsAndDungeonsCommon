using UnityEngine;
using UnityEngine.Serialization;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerBaseAnimator : EntityBaseAnimator
    {
        public CharacterController characterController;
        public GameObject footFX;
        private Vector3 forwardDirection;
        public ParticleSystem[] runDust;

        private Vector3 oldAngles;

        public void Start()
        {
            forwardDirection = new Vector3(0, 0, 0);
            runDust = transform.Find("FootFX").GetComponentsInChildren<ParticleSystem>();
        }


        public PlayerController GetController()
        {
            return (PlayerController) entityController;
        }

        public void RaiseShield()
        {
            animator.SetBool("RaiseShield",true);
        }

        public void LowerShield()
        {
            animator.SetBool("RaiseShield",false);
        }


        private bool playingDust = false;
        
        private void StartDust()
        {
            footFX.SetActive(true);
        }

        private void StopDust()
        {
            
            footFX.SetActive(false);
        }
        
        public override void Update()
        {
            base.Update();
            //simplify in future to just moving somehow
	    animator.SetFloat("SpeedY", ((Player) me).speedDirection.y);
	    animator.SetBool("Grounded",((Player) me).isGrounded);	
	    if(GetController().speedY > 0 && GetController().inClimbing)
	    {
		
	    }
	    if(((Player) me).isGrounded && ((Player) me).speedDirection.y > 0)
	    {
	    	animator.SetTrigger("Jump");
	    }

        if (speed > 0 && ((Player) me).isGrounded)
            StartDust();
        else
            StopDust();
        }


        public void StartAim()
        {
            animator.SetBool("Aim", true);
        }

        public void StopAim()
        {
            animator.SetBool("Aim", false);
        }
        
        
        public void ChangeitemSetting(int leftSide)
        {


            // WE NEED TO DETERMINE ITEM TYPE FROM USED IN FUTURE

            if (leftSide == 0)
            {
                oldAngles = ((Player) me).GetLeftHandHandle().slot.transform.localEulerAngles;

                ((Player) me).GetLeftHandHandle().slot.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                oldAngles = ((Player) me).GetRightHandHandle().transform.localEulerAngles;


                ((Player) me).GetRightHandHandle().slot.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
        }

        public void ChangeItemSettingBack(int leftSide)
        {
            if (leftSide == 0)
                ((Player) me).GetLeftHandHandle().slot.transform.localEulerAngles = oldAngles;
            else
                ((Player) me).GetRightHandHandle().slot.transform.localEulerAngles = oldAngles;
        }
    }
}
