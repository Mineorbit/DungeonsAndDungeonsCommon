using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerBaseAnimator : MonoBehaviour
    {
        public CharacterController characterController;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 forwardDirection = characterController.velocity;
            float angleY = (180/Mathf.PI) * Mathf.Atan2(forwardDirection.x,forwardDirection.z);

            transform.eulerAngles = (new Vector3(0,angleY,0));
        }
    }
}