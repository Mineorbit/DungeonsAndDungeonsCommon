using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityNetworkHandler : LevelObjectNetworkHandler
    {
        public Entity me;



        public virtual void Awake()
        {
            base.Awake();
            me = GetComponent<Entity>();
            //base.AddMethodMarshalling(me.UseSelectedHandle);
        }


        

        void Start()
        {
            if(isOnServer)
            { 
            RequestCreation();
            }
        }

        public virtual void RequestCreation()
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            // CREATE REQUEST
        }

        public static void OnCreationRequest(string identity, LevelObjectData entityType, Vector3 position, Quaternion rotation)
        {
            GameObject e = LevelManager.currentLevel.AddDynamic(entityType,position,rotation);
            e.GetComponent<EntityNetworkHandler>().Identity = identity;
        }

        //UPDATE LOCOMOTION COUPLED WITH TICKRATE
        void FixedUpdate()
        {

        }
    }
}