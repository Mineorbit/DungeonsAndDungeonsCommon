using System;
using System.Collections.Generic;
using System.Threading;
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

        [PacketBinding.SyncVar]
        public int Health {get;set;}
        
        
        [PacketBinding.SyncVar]
        public bool Active {get;set;}
        
        [PacketBinding.SyncVar]
        public int Points {get;set;}
        

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
            if (NetworkManager.instance.isOnServer) RequestCreation();
            
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
            NetworkManager.instance.Server.SendToAll(create);
            
        }
        
        public virtual  void RequestRemoval()
        {
            Entity e = GetObservedEntity();
            Message remove = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.removeEntity);
            remove.AddInt(e.Identity);
            NetworkManager.instance.Server.SendToAll(remove);
        }
        
        public override void Process(int sender, string actionName, List<object> parameters, Message m = null)
        {
            // THIS NEEDS TO HAPPEN BEFORE, BEACUSE p CHANGES THE SENDER NUMBER
            base.Process(sender,actionName,parameters);

            if (NetworkManager.instance.isOnServer)
            {
                NetworkManager.instance.Server.SendToAll(m);
            }
        }
        
        
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.removeEntity)]
        public static void OnEntityRemove(Message value)
        {
            int identity = value.GetInt();
            
                MainCaller.Do(() => { LevelManager.currentLevel.RemoveDynamic(NetworkHandler.ByIdentity(identity).GetObserved()); });
        
        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.createEntity)]
        public static void HandleCreatePacket(Message message)
        {

            int identity = message.GetInt();
            Vector3 position = message.GetVector3();
            int objectType = message.GetInt();
                    LevelObjectData entityLevelObjectData;
                    if (LevelDataManager.levelObjectDatas.TryGetValue(objectType,
                        out entityLevelObjectData))
                    {
                        MainCaller.Do(() =>
                        {
                        OnCreationRequest(identity, entityLevelObjectData, position,
                                new Quaternion(0, 0, 0, 0));
                        });
                    }
                    else
                    {
                        GameConsole.Log($"Could not create Entity of type {objectType}");
                    }
            
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
                return base.CallActionOnOther(localCond, serverCond) && (NetworkManager.instance.isOnServer || IsOwner());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        
        public override bool AcceptAction(int localIdSender)
        {
            if (NetworkManager.instance.localId == -1 && localIdSender != owner)
            {
                return false;
            }
            return true;
        }
        
        
        
        

    }
}
