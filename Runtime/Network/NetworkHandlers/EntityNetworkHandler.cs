using Game;
using General;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityNetworkHandler : LevelObjectNetworkHandler
    {

        public bool isOwner;
        public int owner = -1;

        public new Entity observed;

        public Vector3 targetPosition;
        public Vector3 targetRotation;

        public bool movementOverride;

        public virtual void Awake()
        {
            base.Awake();
            isOwner = isOnServer;
            targetPosition = transform.position;
            targetRotation = transform.rotation.eulerAngles;

            
        }


        

        public override void Start()
        {
            base.Start();
            if (isOnServer)
            {
                RequestCreation();
            }

            observed.onSpawnEvent.AddListener(Teleport);
            observed.onSpawnEvent.AddListener((x) => { UpdateState(); });
            observed.onHitEvent.AddListener((x) => { UpdateState(); });
            observed.onDespawnEvent.AddListener(()=> { Teleport(new Vector3(0, 0, 0)); });
            observed.onDespawnEvent.AddListener(() => { UpdateState(); });
        }

        public virtual void RequestCreation()
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

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
                    if(LevelDataManager.levelObjectDatas.TryGetValue(entityCreate.LevelObjectDataType,out entityLevelObjectData))
                    {
                    Debug.Log("handling spawning");
                    Vector3 position = new Vector3(entityCreate.X,entityCreate.Y,entityCreate.Z);
                        Thread t = new Thread ( new ThreadStart( () => { OnCreationRequest(entityCreate.Identity, entityLevelObjectData, position, new Quaternion(0, 0, 0, 0)); }));
                        t.IsBackground = true;
                        t.Start();
                    }
                }
            });
        }

        public static void OnCreationRequest(string identity, LevelObjectData entityType, Vector3 position, Quaternion rotation)
        {
            //Janky
            Level.levelReady.WaitOne();

            MainCaller.Do(() =>
            {
                Debug.Log("Level: " + LevelManager.currentLevel);
                GameObject e = LevelManager.currentLevel.AddDynamic(entityType, position, rotation);
                e.GetComponent<EntityNetworkHandler>().Identity = identity;
            });
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




        public void UpdateState()
        {
            EntityState entityState = new EntityState
            {
                Health = observed.health,
                Active = observed.gameObject.activeSelf
            };
            Marshall(entityState);
        }

        [PacketBinding.Binding]
        public void OnEntityState(Packet p)
        {
            EntityState entityState;
            if(p.Content.TryUnpack<EntityState>(out entityState))
            {
                observed.health = entityState.Health;
                if(observed.gameObject.activeSelf != entityState.Active)
                {
                    observed.gameObject.SetActive(entityState.Active);
                }
            }
        }



        Vector3 teleportPosition;

        public void Teleport(Vector3 position)
        {
            teleportPosition = position;
            EntityTeleport entityTeleport = new EntityTeleport
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            };



            movementOverride = true;
            observed.setMovementStatus(false);


            if (observed.loadTarget != null) { observed.loadTarget.WaitForChunkLoaded(teleportPosition); }
            Marshall(entityTeleport);

        }




        [PacketBinding.Binding]
        public void OnEntityTeleport(Packet p)
        {
            EntityTeleport entityTeleport;

            if(p.Content.TryUnpack<EntityTeleport>(out entityTeleport))
            {
                teleportPosition = new Vector3(entityTeleport.X,entityTeleport.Y,entityTeleport.Z);


                movementOverride = true;
                observed.setMovementStatus(false);

                if (observed.loadTarget != null) { observed.loadTarget.WaitForChunkLoaded(teleportPosition); }
            }
        }


        float tpDist = 0.005f;

        


        float maxInterpolateDist = 5f;
        public void Update()
        {
            if(movementOverride)
            {
                targetPosition = teleportPosition;
                transform.position = teleportPosition;
                if ((transform.position - teleportPosition).magnitude < tpDist)
                {
                    movementOverride = false;
                    observed.setMovementStatus(true);
                }
            }

            


            if (!isOwner)
            {
                transform.position = (transform.position + targetPosition) / 2;
                transform.rotation = Quaternion.Lerp(Quaternion.Euler(targetRotation.x,targetRotation.y,targetRotation.z),transform.rotation,0.5f);
            }
        }

        Vector3 lastSentPosition;
        Quaternion lastSentRotation;

        float sendDistance = 0.05f;
        private void UpdateLocomotion()
        {
            if(identified && (isOnServer || isOwner))
            {
                Vector3 pos = observed.transform.position;
                Vector3 rot = observed.transform.rotation.eulerAngles;

                float sendDist = (pos - lastSentPosition).magnitude;
                if(sendDist>sendDistance)
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