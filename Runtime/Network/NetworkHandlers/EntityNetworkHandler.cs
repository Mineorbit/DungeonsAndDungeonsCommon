using System;
using System.Threading;
using Game;
using General;
using UnityEngine;
using Google.Protobuf.WellKnownTypes;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityNetworkHandler : LevelObjectNetworkHandler
    {
    
        public bool isSetup = false;
        public int owner = -1;
        
        public Vector3 targetPosition;
        public Vector3 targetRotation;


        public Vector3 lastSentPosition;
        public Quaternion lastSentRotation;

        public readonly float sendDistance = 0.05f;

        public Vector3 teleportPosition;

        public bool transmitPosition = true;
        public bool interpolatePosition = true;

        public virtual void Awake()
        {
            	base.Awake();
            
            	targetPosition = transform.position;
            	targetRotation = transform.rotation.eulerAngles;
        }

        public Entity GetObservedEntity()
        {
            if (observed == null)
            {
                GameConsole.Log($"{gameObject.name} has no observed Entity in EntityNetworkHandler");
            }
            return (Entity) observed;
        }

        
        
        public bool IsOwner()
        {
            return NetworkManager.instance.localId == owner;
        }

        
        public virtual void Start()
        {
			// Request the creation of this entity on the client side
            if (isOnServer) RequestCreation();
            
            
            GetObservedEntity().ApplyMovement = IsOwner();

            GetObservedEntity().onTeleportEvent.AddListener(Teleport);
            GetObservedEntity().onSpawnEvent.AddListener(x => {GameConsole.Log("Spawn State Update"); UpdateState(); });
            GetObservedEntity().onHitEvent.AddListener(x => {GameConsole.Log("Hit State Update"); UpdateState(); });
            GetObservedEntity().onDespawnEvent.AddListener(() => {});
            GetObservedEntity().onDespawnEvent.AddListener(() => {GameConsole.Log("Despawn State Update"); UpdateState(); });
            GetObservedEntity().onPointsChangedEvent.AddListener((x) => {
                if (isOnServer)
                {
                    GameConsole.Log("Points changed State Update " + x);
                    UpdateState();
                }
            });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Level.instantiateType == Level.InstantiateType.Play)
                RequestRemoval();
        }

      

        
        public readonly float tpDist = 0.075f;



        public virtual void Update()
        {
            if(NetworkManager.instance != null)
            {
                    targetPosition = receivedPosition;
                    if (owner != NetworkManager.instance.localId && interpolatePosition)
                    {
                        transform.position = (transform.position + targetPosition) / 2;
                        transform.rotation = Quaternion.Lerp(Quaternion.Euler(targetRotation.x, targetRotation.y, targetRotation.z), transform.rotation, 0.5f*Time.deltaTime);
                    }
                    else
                    {
                        receivedPosition = transform.position;
                        targetPosition = transform.position;
                        targetRotation = transform.rotation.eulerAngles;
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
                Identity = ((NetworkLevelObject)observed).Identity,
                X = position.x,
                Y = position.y,
                Z = position.z,
                LevelObjectDataType = GetObservedEntity().levelObjectDataType
            };

            Marshall(0,entityCreate);
        }
        
        public virtual void RequestRemoval()
        {
            Packet p = GenerateRemovalRequest();
            Server.instance.WriteAll(p);
        }

        public Packet GenerateRemovalRequest()
        {
            var entityRemove = new EntityRemove
            {
                Identity = GetObservedEntity().Identity
            };

            var packet = new Packet
            {
                Type = typeof(EntityRemove).FullName,
                Handler = typeof(EntityNetworkHandler).FullName,
                Content = Any.Pack(entityRemove),
                Identity = GetObservedEntity().Identity
            };
            return packet;
        }
        
        [PacketBinding.Binding]
        public void OnEntityRemove(Packet value)
        {
            EntityRemove entityRemove;
            if (value.Content.TryUnpack(out entityRemove))
            {
                MainCaller.Do(() => { LevelManager.currentLevel.RemoveDynamic(GetObservedEntity()); });
            }
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

        public static void OnCreationRequest(int identity, LevelObjectData entityType, Vector3 position,
            Quaternion rotation)
        {
            System.Action todo = () => MainCaller.Do(() =>
            {
				Util.Optional<int> id = new Util.Optional<int>();
				id.Set(identity);
                GameConsole.Log($"Spawning Entity {entityType}:{identity} at {position} on Network Request");
                var e = LevelManager.currentLevel.AddDynamic(entityType, position, rotation, id);
                e.GetComponent<EntityNetworkHandler>().receivedPosition = position;
                e.GetComponent<EntityNetworkHandler>().targetPosition = position;
                e.GetComponent<EntityNetworkHandler>().targetRotation = rotation.eulerAngles;
            });
            if (LevelManager.currentLevel != null)
            {
                todo.DynamicInvoke();
            }
            else
            {
                Level.entityCreation.Enqueue(todo);
            }
            
        }

        public void CreateTeleportationCompletion()
        {
            var teleportationCompletion = new EntityTeleportComplete
            {
                LastLocomotionId = locomotionID
            };
            Marshall(((NetworkLevelObject) observed).Identity,teleportationCompletion);
        }

        private ulong lastTeleportLocomotionID;
        
        [PacketBinding.Binding]
        public void EntityTeleportComplete(Packet value)
        {
            MainCaller.Do(() =>
            {
                EntityTeleportComplete entityTeleportComplete;
                if (value.Content.TryUnpack(out entityTeleportComplete))
                {
                    lastTeleportLocomotionID = entityTeleportComplete.LastLocomotionId;
                }
            });
        }
        
        public Vector3 receivedPosition;

        private ulong lastReceivedLocomotion;
        [PacketBinding.Binding]
        public void OnEntityLocomotion(Packet p)
        {
            // THIS IS A TEMP
                EntityLocomotion entityLocomotion;
                if (p.Content.TryUnpack(out entityLocomotion))
                {
                    if(entityLocomotion.LocomotionId>lastReceivedLocomotion && entityLocomotion.LocomotionId >lastTeleportLocomotionID)
                    {
                        lastReceivedLocomotion = entityLocomotion.LocomotionId;
                        MainCaller.Do(() =>
                        {
                        var pos = new Vector3(entityLocomotion.X, entityLocomotion.Y, entityLocomotion.Z);
                        receivedPosition = pos;
                        var rot = new Vector3(entityLocomotion.QX, entityLocomotion.QY, entityLocomotion.QZ);
                        targetRotation = rot;
                        ((Entity) observed).aimRotation = new Quaternion(entityLocomotion.AimX, entityLocomotion.AimY,
                            entityLocomotion.AimZ, entityLocomotion.AimW);
                        });
                    }
                }
        }


        public void UpdateState()
        {
            var entityState = new EntityState
            {
                Health = GetObservedEntity().health,
                Active = observed.gameObject.activeSelf,
                Points = GetObservedEntity().points
            };
            GameConsole.Log("Updated State "+entityState);
            Marshall(((NetworkLevelObject) observed).Identity,entityState);
        }

        [PacketBinding.Binding]
        public void OnEntityState(Packet p)
        {
            EntityState entityState;
            if (p.Content.TryUnpack(out entityState))
            {
                GetObservedEntity().health = entityState.Health;
                GetObservedEntity().points = entityState.Points;
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
            
            if (GetObservedEntity().loadTarget != null)
                if(LevelManager.currentLevel != null)
                {
                    GetObservedEntity().loadTarget.WaitForChunkLoaded(teleportPosition, () =>
                {
                    GameConsole.Log($"Sending Teleport: {entityTeleport}");
                    Marshall(((NetworkLevelObject) observed).Identity,entityTeleport);
                });
                }
                else
                {
                    GameConsole.Log($"Sending Teleport: {entityTeleport}");
                    Marshall(((NetworkLevelObject) observed).Identity,entityTeleport);
                }
            }
        }


        
        [PacketBinding.Binding]
        public void OnEntityTeleport(Packet p)
        {
            if(!isOnServer)
            { 
                EntityTeleport entityTeleport;

                if (p.Content.TryUnpack(out entityTeleport))
                    if (GetObservedEntity().loadTarget != null)
                        teleportPosition = new Vector3(entityTeleport.X, entityTeleport.Y, entityTeleport.Z);

                GetObservedEntity().loadTarget.WaitForChunkLoaded(teleportPosition, () =>
                {
                    GetObservedEntity().Teleport(teleportPosition);
                });
                CreateTeleportationCompletion();
            }
        }

        public virtual bool WantsToTransmit()
        {
            return (isOnServer || owner == NetworkManager.instance.localId);
        }

        public float sendAngle = 0.05f;

        private int timeStep = 0;
        private int minSend = 256;
        public virtual bool SendNecessary()
        {
            var pos = observed.transform.position;
            var rot = observed.transform.rotation;
            var sendDist = (pos - lastSentPosition).magnitude;
            var sendRotAngle = Quaternion.Angle(rot, lastSentRotation);
            timeStep++;
            bool needMinimalSend = false;
            if (timeStep > minSend)
            {
                needMinimalSend = true;
                timeStep = 0;
            }
            return sendDist > sendDistance || sendRotAngle > sendAngle || needMinimalSend;
        }

        public override bool CallActionOnOther(bool localCond, bool serverCond)
        {
            try
            {
                return base.CallActionOnOther(localCond, serverCond) && (isOnServer || IsOwner());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        
        public override bool AcceptAction(Packet p)
        {
            if (NetworkManager.instance.localId == -1 && p.Sender != owner) return false;
            else
            {
                return true;
            }
        }
        
        public Quaternion lastSentAimRotation;
        private ulong locomotionID;
        private void UpdateLocomotion()
        {
            if (transmitPosition&&((NetworkLevelObject)observed).identified && WantsToTransmit() && SendNecessary())
            {
                var pos = observed.transform.position;
                var r = observed.transform.rotation;
                var rot = observed.transform.rotation.eulerAngles;
                var aim = ((Entity) observed).aimRotation;
                    var entityLocomotion = new EntityLocomotion
                    {
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        QX = rot.x,
                        QY = rot.y,
                        QZ = rot.z,
                        AimX = aim.x,
                        AimY = aim.y,
                        AimZ = aim.z,
                        AimW = aim.w,
                        LocomotionId = locomotionID
                    };
                    locomotionID++;
                    if (GetObservedEntity().allowedToMove)
                    {
                        Marshall(((NetworkLevelObject) observed).Identity,entityLocomotion, TCP: false,true);
                    }else
                    {
                        Marshall(((NetworkLevelObject)observed).Identity,entityLocomotion, owner,toOrWithout: false, TCP: false,true);
                    }
                    lastSentPosition = pos;
                    lastSentRotation = r;
                    lastSentAimRotation = aim;
            }
        }
    }
}
