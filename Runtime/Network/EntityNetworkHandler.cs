using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityNetworkHandler : NetworkHandler
    {
        public Entity me;

        public string identifier;



        void Start()
        {
            me = GetComponent<Entity>();
            //if(Currently Server)
            //{ 
            RequestCreation();
            //}
        }

        public virtual void RequestCreation()
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            //Send request to the spawner
        }

        public static void OnCreationRequest(string identifier, LevelObjectData entityType, Vector3 position, Quaternion rotation)
        {
            GameObject e = LevelManager.currentLevel.AddDynamic(entityType,position,rotation);
        }

        //UPDATE LOCOMOTION
        void FixedUpdate()
        {

        }
    }
}