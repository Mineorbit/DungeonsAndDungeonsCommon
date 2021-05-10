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

        public virtual void Awake()
        {
            base.Awake();
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
            if(!isOwner || (dist < maxInterpolateDist))
            {
                transform.position = (transform.position + targetPosition) / 2;
                transform.rotation = Quaternion.Lerp(transform.rotation,targetRotation,0.5f);
            }else if(!isOwner || (dist >= maxInterpolateDist))
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
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