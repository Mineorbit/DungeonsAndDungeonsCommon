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
        Quaternion targetRotation;

        [PacketBinding.Binding]
        public void OnEntityLocomotion(Packet p)
        {
            EntityLocomotion entityLocomotion;
            if(p.Content.TryUnpack<EntityLocomotion>(out entityLocomotion))
            {
                MainCaller.Do(() => {
                    Vector3 pos = new Vector3(entityLocomotion.X, entityLocomotion.Y, entityLocomotion.Z);
                    targetPosition = pos;
                    Quaternion rot = new Quaternion(entityLocomotion.QX, entityLocomotion.QY, entityLocomotion.QZ, entityLocomotion.QW);
                    targetRotation = rot;
                    Debug.Log("TEST "+pos+" "+rot);
                });
            }
        }

        float maxInterpolateDist = 5f;
        public void Update()
        {

            float dist = (transform.position - targetPosition).magnitude;

            if ((!isOwner || isOnServer) && (dist < maxInterpolateDist))
            {
                networkPosition = (networkPosition + targetPosition) / 2;
                networkRotation = Quaternion.Lerp(targetRotation, networkRotation, 0.5f);
            }
            else
            if ( !isOnServer && isOwner && (dist >= maxInterpolateDist))
            {

                networkPosition = targetPosition;
                networkRotation = targetRotation;
            }
            

        }

        private void UpdateLocomotion()
        {
            if(identified && (isOnServer || isOwner))
            {
                Vector3 pos = observed.transform.position;
                Quaternion rot = observed.transform.rotation;
                EntityLocomotion entityLocomotion = new EntityLocomotion
                {
                    X = pos.x,
                    Y = pos.y,
                    Z = pos.z,
                    QX = rot.x,
                    QY = rot.y,
                    QZ = rot.z,
                    QW = rot.w
                };
                Marshall(entityLocomotion);
            }
        }
    }
}