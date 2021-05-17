using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerBaseAnimator : EntityBaseAnimator
    {
        public CharacterController characterController;
        public PlayerController playerController;
        public GameObject footFX;
        private Vector3 forwardDirection;

        private Vector3 oldAngles;

        private void Start()
        {
            forwardDirection = new Vector3(0, 0, 0);
        }

        public override void Update()
        {
            base.Update();

            speed = playerController.currentSpeed;


            //simplify in future to just moving somehow

            if (me == PlayerManager.currentPlayer)
                footFX.SetActive(playerController.IsGrounded && playerController.allowedToMove &&
                                 (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) ||
                                  Input.GetKey(KeyCode.D)));
            animator.SetFloat("Speed", speed);
        }

        public void ChangeitemSetting(int leftSide)
        {
            Debug.Log("Changed Setting");


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
            Debug.Log("Changed Setting back");
            if (leftSide == 0)
                ((Player) me).GetLeftHandle().slot.transform.localEulerAngles = oldAngles;
            else
                ((Player) me).GetRightHandle().slot.transform.localEulerAngles = oldAngles;
        }
    }
}