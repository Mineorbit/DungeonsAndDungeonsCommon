using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.WellKnownTypes;
using NetLevel;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.Events;
using Type = System.Type;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectNetworkHandler : NetworkHandler
    {

        private List<string> availableActions = new List<string>();

        public bool disabled_observed = true;

        public UnityEvent valueChangedEvent = new UnityEvent();
        
        [PacketBinding.SyncVar]
        public int AnimatorState {get;set;}


        public virtual LevelObject GetObserved()
        {
            return (LevelObject) observed;
        }

        
        
        public override void Awake()
        {
            observed = GetComponent<NetworkLevelObject>();
            base.Awake();
            // currently not updated
            if (observed != null)
                GetObserved().enabled = !disabled_observed || (!NetworkManager.isConnected || NetworkManager.instance.isOnServer);
            
            NetworkManager.networkHandlers.Add(this);
            
        }

        public virtual void Start()
        {
            if(!observed.GetType().IsSubclassOf(typeof(Entity)))
            {

                if (NetworkManager.instance.isOnServer)
                {
                    SetProperties();
                }
            }
        }

        public virtual bool CallActionOnOther(bool localCond, bool serverCond)
        {
            if (NetworkManager.instance.localId == -1)
            {
                return localCond;
            }
            else
            {
                return serverCond;
            }
            
        }


       

        public virtual bool AcceptAction(int senderId)
        {
            return true;
        }
        
        // CALLABLE METHODS MUST BE MARKED PUBLIC TO BE USABLE
        [MessageHandler((ushort)NetworkManager.ServerToClientId.processAction)]
        public static void ProcessAction(Message m)
        {
            int identity = m.GetInt();
            int sender = m.GetInt();
            string actionName = m.GetString();
            string type = m.GetString();
            
            var parameters = new List<object>();
            if (type == "UnityEngine.Vector3")
            {
                parameters.Add(m.GetVector3());
            }
            else
            {
                GameConsole.Log($"Do not know Type {type}");
            }
            LevelObjectNetworkHandler o = NetworkLevelObject.FindByIdentity(identity).levelObjectNetworkHandler;
            if (o != null)
            {
                o.Process(sender,actionName,parameters,m);
            }
            else
            {
                GameConsole.Log($"Could not find {identity} for action call");
            }


        }
        
        
        
        
        // CALLABLE METHODS MUST BE MARKED PUBLIC TO BE USABLE
        [MessageHandler((ushort)NetworkManager.ClientToServerId.processAction)]
        public static void ProcessAction(ushort id,Message m)
        {
            GameConsole.Log("Received Action");
            int identity = m.GetInt();
            int sender = m.GetInt();
            string actionName = m.GetString();
            string type = m.GetString();
            
            var parameters = new List<object>();
            if (type == "UnityEngine.Vector3")
            {
                parameters.Add(m.GetVector3());
            }
            else
            {
                GameConsole.Log($"Do not know Type {type}");
            }
            /*
            while(m.UnreadLength > 0)
            {
            }
            */
            // HIER EIN CHECK OB DER SENDER WIRKLICH DER EIGENTÜMER IST VOM OBJEKT
            LevelObjectNetworkHandler o = NetworkLevelObject.FindByIdentity(identity).levelObjectNetworkHandler;
            if (o != null)
            {
                o.Process(sender,actionName,parameters,m);
            }
            else
            {
                GameConsole.Log($"Could not find {identity}");
            }

        }


        public virtual void Process(int sender, string actionName,List<object> parameters, Message m = null)
        {
            GameConsole.Log($"{sender} asked {this} to do {actionName} ");

            //if (!AcceptAction(sender)) return;
                
            var methodInfo = observed.GetType().GetMethod(actionName);
                
            if (methodInfo != null)
                methodInfo.Invoke(observed, parameters.ToArray());
            else
                GameConsole.Log("The Method with the given name " + actionName +
                                " could not be found, is it private?");
            
        }

        // THIS NEEDS A SAFETY LIST LATER
        
        
        // NOT COMPLETED
        // THIS NEEDS TO PACK ARGUMENTS INTO ANY
        public virtual void SendAction(string actionName, object argument)
        {
            
            
            
            Message actionM;

            actionM = NetworkManager.instance.isOnServer ? Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.processAction) : Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ClientToServerId.processAction);
            actionM.AddInt(((NetworkLevelObject)GetObserved()).Identity);
            actionM.AddInt(NetworkManager.instance.localId);
            actionM.AddString(actionName);
            actionM.AddString(argument.GetType().FullName);
            if (argument.GetType().IsSubclassOf(typeof(Vector3)))
            {
                actionM.AddVector3((Vector3) argument);
            }
            
            if (NetworkManager.instance.isOnServer)
            {
                NetworkManager.instance.Server.SendToAll(actionM);
            }
            else
            {
                NetworkManager.instance.Client.Send(actionM);
            }
        }
        
        public virtual void SetProperties()
        {
                Message propertyMessage = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.setProperty);
                propertyMessage.AddVector3(transform.position);
                propertyMessage.AddInt(((NetworkLevelObject) GetObserved()).Identity);
                foreach (var property in GetObserved().levelObjectProperties)
                {
                    propertyMessage.AddString(property.name);
                    propertyMessage.AddString(property.Value);
                    GameConsole.Log($"Adding Property {property.name} {property.Value} to message");
                }
                GameConsole.Log($"Sending {propertyMessage.UnreadLength}");
                
                
                SendToExisting(propertyMessage);
        }


        public struct RequestTuple
        {
            public Vector3 pos;
            public int identity;
            public List<Tuple<string,string>> properties;
        }

        public static Queue<RequestTuple> propertyRequests = new Queue<RequestTuple>();

        [MessageHandler((ushort) NetworkManager.ServerToClientId.setProperty)]
        public static void ReceiveProperties(Message m)
        {
            Vector3 position = m.GetVector3();
            int id =m.GetInt();
            RequestTuple request = new RequestTuple();
            request.pos = position;
            request.identity = id;
            request.properties = new List<Tuple<string, string>>();
            while (m.UnreadLength > 0)
            {
                string name = m.GetString();
                string value = m.GetString();
                request.properties.Add(new Tuple<string, string>(name,value));
            }
            ProcessPropertyRequest(request);
        }

        

        public static void ProcessPropertyRequest(RequestTuple tuple)
        {
            NetworkHandler target = NetworkManager.networkHandlers.Find((x) =>
            {
                if (x != null)
                {
                    float dist = (tuple.pos - x.transform.position).magnitude;
                    return dist < 0.125f;
                }
                return false;
            });
            if (target != null)
            {           
                NetworkLevelObject o = ((NetworkLevelObject) ((LevelObjectNetworkHandler) target).GetObserved());
                o.OverrideIdentity(tuple.identity);
                foreach (Tuple<string,string> t in tuple.properties)
                {
                    o.GetProperty(t.Item1).Value = t.Item2;
                }
            }
            else
            {
                propertyRequests.Enqueue(tuple); 
            }
        }
        
        public virtual void SendAction(string actionName, ChunkData argument)
        {
            
        }

        
        public void SendAction(string actionName)
        {
            Message actionM = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.processAction);

            
            actionM.AddInt(((NetworkLevelObject)GetObserved()).Identity);
            actionM.AddInt(NetworkManager.instance.localId);
            actionM.AddString(actionName);
            actionM.AddString("None");

            if (NetworkManager.instance.isOnServer)
            {
                NetworkManager.instance.Server.SendToAll(actionM);
            }
            else
            {
                NetworkManager.instance.Client.Send(actionM);
            }
        }



    }
}
