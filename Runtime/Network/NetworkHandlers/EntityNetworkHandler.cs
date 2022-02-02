using System;
using System.Threading;
using Game;
using General;
using UnityEngine;
using Google.Protobuf.WellKnownTypes;
using RiptideNetworking;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class EntityNetworkHandler : LevelObjectNetworkHandler
    {
    
        public int owner = -1;
        


        public Vector3 lastSentPosition;
        public Quaternion lastSentRotation;

        
        public readonly float tpDist = 0.075f;

        // THIS IS NEEDED FOR PROPERTIES
        [PacketBinding.SyncVar]
        public Vector3 receivedPosition {get;set;}
        
        [PacketBinding.SyncVar]
        public Vector3 receivedRotation {get;set;}
        
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
            else
            {
                receivedPosition = transform.position;
                receivedRotation = transform.rotation.eulerAngles;
            }
        }

       

        public virtual void RequestCreation()
        {
            var position = transform.position;
           
            Message create = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.createEntity);

            create.AddInt(GetObservedEntity().Identity);
            create.AddVector3(position);
            create.AddInt(GetObservedEntity().levelObjectDataType);
            NetworkManager.instance.server.SendToAll(create);
            
        }
        
        public  void RequestRemoval()
        {
            Entity e = GetObservedEntity();
            Message remove = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.removeEntity);
            remove.AddInt(e.Identity);
            NetworkManager.instance.server.SendToAll(remove);
        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.processAction)]
        public void ProcessAction(Message m)
        {
            // THIS NEEDS TO HAPPEN BEFORE, BEACUSE p CHANGES THE SENDER NUMBER
            base.ProcessAction(m);

            if (isOnServer)
            {
                NetworkManager.instance.server.SendToAll(m);
            }
        }
        
        
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.removeEntity)]
        public void OnEntityRemove(Packet value)
        {
            EntityRemove entityRemove;
            if (value.Content.TryUnpack(out entityRemove))
            {
                MainCaller.Do(() => { LevelManager.currentLevel.RemoveDynamic(GetObservedEntity()); });
            }
        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.createEntity)]
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

        [MessageHandler((ushort)NetworkManager.ServerToClientId.entityState)]
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
        
        
        
        

    }
}
