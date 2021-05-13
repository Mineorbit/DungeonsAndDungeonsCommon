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


        Vector3 targetPosition;
        Vector3 targetRotation;
        public virtual void Awake()
        {
            base.Awake();
            isOwner = isOnServer;
            targetPosition = transform.position;
            targetRotation = transform.rotation.eulerAngles;
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

            EntityCreate entityCreate = new EntityCreate
            {
                Identity = this.Identity,
                X = position.x,
                Y = position.y,
                Z = position.z,
                LevelObjectDataType = observed.levelObjectDataType
            };

            Marshall(entityCreate);

        }


        public virtual void RequestRemoval()
        {

        }

        [PacketBinding.Binding]
        public static void HandleCreatePacket(Packet value)
        {
            MainCaller.Do(() =>
            {
                Debug.Log("handling spawning");
                EntityCreate entityCreate;
                if (value.Content.TryUnpack<EntityCreate>(out entityCreate))
                {
                
                    LevelObjectData entityLevelObjectData;
                    if(LevelManager.currentLevel.levelObjectDatas.TryGetValue(entityCreate.LevelObjectDataType,out entityLevelObjectData))
                    {
                    Debug.Log("handling spawning");
                    Vector3 position = new Vector3(entityCreate.X,entityCreate.Y,entityCreate.Z);
                    OnCreationRequest(entityCreate.Identity,entityLevelObjectData,position,new Quaternion(0,0,0,0));
                    }
                }
            });
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