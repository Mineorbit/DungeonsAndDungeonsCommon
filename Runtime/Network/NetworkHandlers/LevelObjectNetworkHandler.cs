using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.WellKnownTypes;
using NetLevel;
using RiptideNetworking;
using UnityEngine;
using Type = System.Type;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectNetworkHandler : NetworkHandler
    {

        private List<string> availableActions = new List<string>();

        public bool disabled_observed = true;


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
            
            if(!observed.GetType().IsSubclassOf(typeof(Entity)))
            {
                SetProperties();
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
            LevelObjectNetworkHandler o = ByIdentity(identity);
            if (o != null)
            {
                o.Process(sender,actionName,parameters,m);
            }


        }
        
        // CALLABLE METHODS MUST BE MARKED PUBLIC TO BE USABLE
        [MessageHandler((ushort)NetworkManager.ServerToClientId.processAction)]
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
            LevelObjectNetworkHandler o = ByIdentity(identity);
            if (o != null)
            {
                o.Process(sender,actionName,parameters,m);
            }

        }


        public virtual void Process(int sender, string actionName,List<object> parameters, Message m = null)
        {
            GameConsole.Log($"{sender} asked {this} to do {actionName} ");

            if (!AcceptAction(sender)) return;
                
            var methodInfo = observed.GetType().GetMethod(actionName);
                
            if (methodInfo != null)
                MainCaller.Do(() => { methodInfo.Invoke(observed, parameters.ToArray()); });
            else
                GameConsole.Log("The Method with the given name " + actionName +
                                " could not be found, is it private?");
            
        }

        // THIS NEEDS A SAFETY LIST LATER
        
        
        // NOT COMPLETED
        // THIS NEEDS TO PACK ARGUMENTS INTO ANY
        public virtual void SendAction(string actionName, object argument)
        {
            
            
            
            Message actionM = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.processAction);

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
            if(NetworkManager.instance.isOnServer)
            {
                Message propertyM = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.setProperty);
                propertyM.AddVector3(transform.position);
                propertyM.AddInt(((NetworkLevelObject) GetObserved()).Identity);
                NetworkManager.instance.Server.SendToAll(propertyM);
            }
        }


        public struct RequestTuple
        {
            public Vector3 pos;
            public int identity;
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
            ProcessPropertyRequest(request);
            
        }

        

        public static void ProcessPropertyRequest(RequestTuple tuple)
        {
            NetworkHandler target = NetworkManager.networkHandlers.Find((x) =>
            {
                float dist = (tuple.pos - x.transform.position).magnitude;
                GameConsole.Log($"Dist: {dist}");
                return  dist < 0.125f;
            });
            GameConsole.Log($"Position {tuple.pos} {target}");
            if (target != null)
            {           
                NetworkLevelObject o = ((NetworkLevelObject) ((LevelObjectNetworkHandler) target).GetObserved());

                o.Identity = tuple.identity;
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
