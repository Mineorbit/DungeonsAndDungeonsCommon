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
    
        public int owner = -1;
        


        public Vector3 lastSentPosition;
        public Quaternion lastSentRotation;

        
        public readonly float tpDist = 0.075f;

        public Vector3 receivedPosition;
        public Vector3 receivedRotation;
        
        public virtual void Awake()
        {
            	base.Awake();
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

      

        public virtual void Update()
        {
            if(NetworkManager.instance != null && -1 != NetworkManager.instance.localId)
            {
                transform.position = (transform.position + receivedPosition) / 2;
                transform.rotation = Quaternion.Lerp(Quaternion.Euler(receivedRotation.x, receivedRotation.y, receivedRotation.z), transform.rotation, 0.5f*Time.deltaTime);
            }
        }

        //UPDATE LOCOMOTION COUPLED WITH TICKRATE
        public void FixedUpdate()
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
                        OnCreationRequest(entityCreate.Identity, entityLevelObjectData, position,
                                new Quaternion(0, 0, 0, 0));
                        
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

       
        

        private ulong lastReceivedLocomotion;
        [PacketBinding.Binding]
        public virtual void OnEntityLocomotion(Packet p)
        {
            // THIS IS A TEMP
                EntityLocomotion entityLocomotion;
                if (p.Content.TryUnpack(out entityLocomotion))
                {
                    if (entityLocomotion.LocomotionId > lastReceivedLocomotion)
                    {
                        lastReceivedLocomotion = entityLocomotion.LocomotionId;
                        MainCaller.Do(() =>
                        {
                        var pos = new Vector3(entityLocomotion.X, entityLocomotion.Y, entityLocomotion.Z);
                        receivedPosition = pos;
                        var rot = new Vector3(entityLocomotion.QX, entityLocomotion.QY, entityLocomotion.QZ);
                        receivedRotation = rot;
                        ((Entity) observed).aimRotation = new Quaternion(entityLocomotion.AimX, entityLocomotion.AimY,
                            entityLocomotion.AimZ, entityLocomotion.AimW);
                        });
                    }
                }
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawSphere(receivedPosition,0.25f);
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

        public virtual bool WantsToTransmit()
        {
            return ( -1 == NetworkManager.instance.localId);
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
            return sendDist > 0.01f || sendRotAngle > sendAngle;
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
            if (NetworkManager.instance.localId == -1 && p.Sender != owner)
            {
                return false;
            }
            return true;
        }
        
        
        
        
        
        public Quaternion lastSentAimRotation;
        private ulong locomotionId = 0;
        private void UpdateLocomotion()
        {
            if (WantsToTransmit() && SendNecessary())
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
                        LocomotionId = locomotionId
                    };
                    locomotionId++;
                    Marshall(((NetworkLevelObject) observed).Identity,entityLocomotion, TCP: false,true);

                    lastSentPosition = pos;
                    lastSentRotation = r;
                    lastSentAimRotation = aim;
            }
        }
    }
}
