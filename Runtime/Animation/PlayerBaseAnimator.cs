using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerBaseAnimator : EntityBaseAnimator
    {
        public CharacterController characterController;
        public PlayerController playerController;
        public GameObject footFX;
        Vector3 forwardDirection;

        void Start()
        {
            forwardDirection = new Vector3(0,0,0);
        }

        Vector3 oldAngles;

        void ChangeitemSetting()
        {
            Debug.Log("Changed Setting");
            oldAngles = me.items[0].transform.localEulerAngles;
            me.items[0].transform.localEulerAngles = new Vector3(0, 0,0);
        }

        void ChangeItemSettingBack()
        {
            Debug.Log("Changed Setting back");
            me.items[0].transform.localEulerAngles = oldAngles;
        }

        void Update()
        {
            float speed = characterController.velocity.magnitude;
            if (speed > 0)
            { 
                forwardDirection =  (forwardDirection + characterController.velocity)/2;
            }

            //simplify in future to just moving somehow

            if(me == PlayerManager.currentPlayer)
            {
                footFX.SetActive((playerController.IsGrounded && playerController.allowedToMove && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))));
            }


            float angleY = 180 + (180/Mathf.PI) * Mathf.Atan2(forwardDirection.x,forwardDirection.z);
            animator.SetFloat("Speed",speed);
            transform.eulerAngles = (new Vector3(0,angleY,0));
        }
    }
}