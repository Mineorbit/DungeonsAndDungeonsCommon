using Game;
using General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityNetworkHandler : LevelObjectNetworkHandler
    {

        public bool isOwner;
        public int owner = -1;

        public Vector3 networkPosition;
        public Quaternion networkRotation;

        public virtual void Awake()
        {
            base.Awake();
            isOwner = isOnServer;
        }


        

        public virtual void Start()
        {
            if (isOnServer)
            {
                RequestCreation();
            }
        }

        public virtual void RequestCreation()
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            Debug.Log("Entity Create");
            // CREATE REQUEST
        }


        public virtual void RequestRemoval()
        {

        }


        public static void OnCreationRequest(string identity, LevelObjectData entityType, Vector3 position, Quaternion rotation)
        {
            GameObject e = LevelManager.currentLevel.AddDynamic(entityType,position,rotation);
            e.GetComponent<EntityNetworkHandler>().Identity = identity;
        }

        //UPDATE LOCOMOTION COUPLED WITH TICKRATE
        void FixedUpdate()
        {
            UpdateLocomotion();
        }

        Vector3 targetPosition;
        Vector3 targetRotation;

        [PacketBinding.Binding]
        public void OnEntityLocomotion(Packet p)
        {
            EntityLocomotion entityLocomotion;
            if(p.Content.TryUnpack<EntityLocomotion>(out entityLocomotion))
            {
                MainCaller.Do(() => {
                    Vector3 pos = new Vector3(entityLocomotion.X, entityLocomotion.Y, entityLocomotion.Z);
                    targetPosition = pos;
                    Vector3 rot = new Vector3(entityLocomotion.QX, entityLocomotion.QY, entityLocomotion.QZ);
                    targetRotation = rot;
                });
            }
        }

        float maxInterpolateDist = 5f;
        public void Update()
        {
            float dist = (transform.position - targetPosition).magnitude;

            if (!isOwner)
            {
                transform.position = (transform.position + targetPosition) / 2;
                transform.rotation = Quaternion.Euler(targetRotation.x,targetRotation.y,targetRotation.z);
            }
        }

        Vector3 lastSentPosition;
        Quaternion lastSentRotation;
        private void UpdateLocomotion()
        {
            if(identified && (isOnServer || isOwner))
            {
                Vector3 pos = observed.transform.position;
                Vector3 rot = observed.transform.rotation.eulerAngles;
                float sendDist = (pos - lastSentPosition).magnitude;
                if(sendDist>0.5f)
                {
                EntityLocomotion entityLocomotion = new EntityLocomotion
                {
                    X = pos.x,
                    Y = pos.y,
                    Z = pos.z,
                    QX = rot.x,
                    QY = rot.y,
                    QZ = rot.z
                };
                // UDP IS BROKEN THIS NEEDS A FIX LATER
                Marshall(entityLocomotion,owner,toOrWithout:false,TCP: true);
                lastSentPosition = pos;
                }
            }
        }
    }
}