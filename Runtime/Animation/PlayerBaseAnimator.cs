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
            runDust = transform.Find("Particles").Find("Running").GetComponentsInChildren<ParticleSystem>();
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
            if (!playingDust)
            {
                playingDust = true;
                runDust[0].Play();
                runDust[1].Play();
            }
        }

        private void StopDust()
        {
            if (playingDust)
            {
                playingDust = false;
                runDust[0].Stop();
                runDust[1].Stop();
            }
        }
        
        public override void Update()
        {
            base.Update();
            //simplify in future to just moving somehow
	    animator.SetFloat("SpeedY",GetController().speedY);
	    animator.SetBool("Grounded",((Player) me).isGrounded);	
	    if(GetController().speedY > 0 && GetController().inClimbing)
	    {
		
	    }
	    if(((Player) me).isGrounded && Input.GetKeyDown(KeyCode.Space))
	    {
	    	animator.SetTrigger("Jump");
	    }
        if (speed > 0 && ((Player) me).isGrounded)
            StartDust();
        else
            StopDust();
        
            if (me == PlayerManager.currentPlayer)
                footFX.SetActive(((Player) me).isGrounded && GetController().allowedToMove &&
                                 (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) ||
                                  Input.GetKey(KeyCode.D)));
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
                oldAngles = ((Player) me).GetLeftHandle().slot.transform.localEulerAngles;

                ((Player) me).GetLeftHandle().slot.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                oldAngles = ((Player) me).GetRightHandle().transform.localEulerAngles;


                ((Player) me).GetRightHandle().slot.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
        }

        public void ChangeItemSettingBack(int leftSide)
        {
            if (leftSide == 0)
                ((Player) me).GetLeftHandle().slot.transform.localEulerAngles = oldAngles;
            else
                ((Player) me).GetRightHandle().slot.transform.localEulerAngles = oldAngles;
        }
    }
}
