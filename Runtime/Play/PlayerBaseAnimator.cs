using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerBaseAnimator : MonoBehaviour
    {
        public CharacterController characterController;
        Vector3 forwardDirection;
        void Start()
        {
            forwardDirection = new Vector3(0,0,0);
        }

        // Update is called once per frame
        void Update()
        {
            forwardDirection =  (forwardDirection + characterController.velocity)/2;
            float angleY = 180 + (180/Mathf.PI) * Mathf.Atan2(forwardDirection.x,forwardDirection.z);

            transform.eulerAngles = (new Vector3(0,angleY,0));
        }
    }
}