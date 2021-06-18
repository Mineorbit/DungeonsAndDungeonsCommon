using System.Threading;
using Game;
using General;
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


        private Vector3 lastSentPosition;
        private Quaternion lastSentRotation;


        private float maxInterpolateDist = 5f;

        private readonly float sendDistance = 0.05f;


        public Vector3 teleportPosition;


        
        

        public override void Awake()
        {
            base.Awake();
            isOwner = isOnServer;
            targetPosition = transform.position;
            targetRotation = transform.rotation.eulerAngles;
        }


        public override void Start()
        {
            base.Start();
            if (isOnServer) RequestCreation();

            observed.onTeleportEvent.AddListener(Teleport);
            observed.onSpawnEvent.AddListener(x => {Debug.Log("Spawn State Update"); UpdateState(); });
            observed.onHitEvent.AddListener(x => {Debug.Log("Hit State Update"); UpdateState(); });
            observed.onDespawnEvent.AddListener(() => { Teleport(new Vector3(0, 0, 0)); });
            observed.onDespawnEvent.AddListener(() => {Debug.Log("Despawn State Update"); UpdateState(); });
            observed.onPointsChangedEvent.AddListener((x) => {
                if (isOnServer)
                {
                    Debug.Log("Points changed State Update " + x);
                    UpdateState();
                }
            });
        }

        void ResolveLocomotionBlock()
        {
            blockExists = false;
            targetPosition = transform.position;
            observed.setMovementStatus(true);
            Debug.Log("Resolve Locomotion Block");
            Debug.Break();
        }

        public bool blockExists = false;
        public Vector3 blockPosition;
        
        void SetupLocomotionBlock(Vector3 bPosition)
        {
            blockExists = true;
            blockPosition = bPosition;
            targetPosition = bPosition;
            observed.setMovementStatus(false);
            Debug.Log("Set Locomotion Block");
            Debug.Break();
        }

        
        bool LocomotionIsBlocked()
        {
            return (blockExists);
        }
        
        
        private readonly float tpDist = 0.005f;
        
        public virtual void Update()
        {
            if (LocomotionIsBlocked())
            {
                targetPosition = blockPosition;
                transform.position = blockPosition;
                if ((targetPosition - receivedPosition).magnitude < tpDist)
                {
                    ResolveLocomotionBlock();
                }

            }
            else
            {
                targetPosition = receivedPosition;
                if (!isOwner)
                {
                    transform.position = (transform.position + targetPosition) / 2;
                    transform.rotation =
                        Quaternion.Lerp(Quaternion.Euler(targetRotation.x, targetRotation.y, targetRotation.z),
                            transform.rotation, 0.5f);
                }
            }

            
        }

        //UPDATE LOCOMOTION COUPLED WITH TICKRATE
        private void FixedUpdate()
        {
            UpdateLocomotion();
        }

        public virtual void RequestCreation()
        {
            var position = transform.position;
            var rotation = transform.rotation;

            var entityCreate = new EntityCreate
            {
                Identity = Identity,
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
                EntityCreate entityCreate;
                if (value.Content.TryUnpack(out entityCreate))
                {
                    LevelObjectData entityLevelObjectData;
                    if (LevelDataManager.levelObjectDatas.TryGetValue(entityCreate.LevelObjectDataType,
                        out entityLevelObjectData))
                    {
                        var position = new Vector3(entityCreate.X, entityCreate.Y, entityCreate.Z);
                        var t = new Thread(() =>
                        {
                            OnCreationRequest(entityCreate.Identity, entityLevelObjectData, position,
                                new Quaternion(0, 0, 0, 0));
                        });
                        t.IsBackground = true;
                        t.Start();
                    }
                }
            });
        }

        public static void OnCreationRequest(string identity, LevelObjectData entityType, Vector3 position,
            Quaternion rotation)
        {
            //Janky
            Level.levelReady.WaitOne();

            MainCaller.Do(() =>
            {
                Debug.Log("Level: " + LevelManager.currentLevel);
                var e = LevelManager.currentLevel.AddDynamic(entityType, position, rotation);
                e.GetComponent<EntityNetworkHandler>().Identity = identity;
            });
        }


        private Vector3 receivedPosition;
        [PacketBinding.Binding]
        public void OnEntityLocomotion(Packet p)
        {
                EntityLocomotion entityLocomotion;
                if (p.Content.TryUnpack(out entityLocomotion))
                {
                    MainCaller.Do(() =>
                    {
                    var pos = new Vector3(entityLocomotion.X, entityLocomotion.Y, entityLocomotion.Z);
                    receivedPosition = pos;
                    var rot = new Vector3(entityLocomotion.QX, entityLocomotion.QY, entityLocomotion.QZ);
                    targetRotation = rot;
                    });
                }
        }


        public void UpdateState()
        {
            var entityState = new EntityState
            {
                Health = observed.health,
                Active = observed.gameObject.activeSelf,
                Points = observed.points
            };
            Marshall(entityState);
        }

        [PacketBinding.Binding]
        public void OnEntityState(Packet p)
        {
            EntityState entityState;
            if (p.Content.TryUnpack(out entityState))
            {
                observed.health = entityState.Health;
                observed.points = entityState.Points;
                if (observed.gameObject.activeSelf != entityState.Active)
                    observed.gameObject.SetActive(entityState.Active);
            }
        }

        
        public void Teleport(Vector3 position)
        {
            if(isOnServer)
            {
            teleportPosition = position;
            var entityTeleport = new EntityTeleport
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            };
            
            SetupLocomotionBlock(teleportPosition);
            
            if (observed.loadTarget != null)
                observed.loadTarget.WaitForChunkLoaded(teleportPosition, () =>
                {
                    Marshall(entityTeleport);
                });
            
            }
        }


        [PacketBinding.Binding]
        public void OnEntityTeleport(Packet p)
        {
            if(!isOnServer)
            { 
                EntityTeleport entityTeleport;

                if (p.Content.TryUnpack(out entityTeleport))
                    if (observed.loadTarget != null)
                        teleportPosition = new Vector3(entityTeleport.X, entityTeleport.Y, entityTeleport.Z);

                SetupLocomotionBlock(teleportPosition);
                observed.loadTarget.WaitForChunkLoaded(teleportPosition, () =>
                {
                    observed.Teleport(teleportPosition);
                    ResolveLocomotionBlock();
                });
            }
        }

        private void UpdateLocomotion()
        {
            if (identified && (isOnServer || isOwner) && !LocomotionIsBlocked())
            {
                var pos = observed.transform.position;
                var rot = observed.transform.rotation.eulerAngles;

                var sendDist = (pos - lastSentPosition).magnitude;
                if (sendDist > sendDistance)
                {
                    var entityLocomotion = new EntityLocomotion
                    {
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        QX = rot.x,
                        QY = rot.y,
                        QZ = rot.z
                    };
                    if (observed.movementOverride)
                    {
                        Debug.Log("Sending Locomotion to everyone");
                        Marshall(entityLocomotion, TCP: false);
                    }else
                    {
                        Debug.Log("Sending Locomotion to everyone except "+owner);
                        Marshall(entityLocomotion, owner,toOrWithout: false, TCP: false);
                    }
                    lastSentPosition = pos;
                }
            }
        }
    }
}