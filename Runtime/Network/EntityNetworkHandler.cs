using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityNetworkHandler : NetworkHandler
    {
        void Start()
        {
            RequestCreation();
        }

        void RequestCreation()
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            //Send request to the spawner
        }

        //UPDATE LOCOMOTION
        void FixedUpdate()
        {

        }
    }
}