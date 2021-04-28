using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerBaseAnimator : EntityBaseAnimator
    {
        public CharacterController characterController;
        Vector3 forwardDirection;
        void Start()
        {
            forwardDirection = new Vector3(0,0,0);
        }

        Vector3 oldAngles;

        public void ChangeitemSetting()
        {
            Debug.Log("Changed Setting");
            oldAngles = me.items[0].transform.localEulerAngles;
            me.items[0].transform.localEulerAngles = new Vector3(0, 0,0);
        }

        public void ChangeItemSettingBack()
        {
            Debug.Log("Changed Setting back");
            me.items[0].transform.localEulerAngles = oldAngles;
        }

        // Update is called once per frame
        void Update()
        {
            float speed = characterController.velocity.magnitude;
            if (speed > 0)
            forwardDirection =  (forwardDirection + characterController.velocity)/2;
            float angleY = 180 + (180/Mathf.PI) * Mathf.Atan2(forwardDirection.x,forwardDirection.z);
            animator.SetFloat("Speed",speed);
            transform.eulerAngles = (new Vector3(0,angleY,0));
        }
    }
}