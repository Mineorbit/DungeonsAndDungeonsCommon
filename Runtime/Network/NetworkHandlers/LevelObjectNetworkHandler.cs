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
            properties = this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PacketBinding.SyncVar))).ToArray();
            base.Awake();
            // currently not updated
            if (observed != null)
                GetObserved().enabled = !disabled_observed || (!NetworkManager.isConnected || NetworkManager.instance.isOnServer);
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
        public virtual void ProcessAction(Message m)
        {
            int sender = m.GetInt();
            string actionName = m.GetString();
            var parameters = new List<object>();
            while(m.UnreadLength > 0)
            {
                parameters.Add(m.GetVector3());
            }
                
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
            
            
            
            Message actionM = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.createEntity);

            actionM.AddString(actionName);

            if (argument.GetType().IsSubclassOf(typeof(Vector3)))
            {
                actionM.AddVector3((Vector3) argument);
            }
            
            if (NetworkManager.instance.isOnServer)
            {
                NetworkManager.instance.server.SendToAll(actionM);
            }
            else
            {
                NetworkManager.instance.client.Send(actionM);
            }
            

            
        }

        public virtual void SendAction(string actionName, ChunkData argument)
        {
            
        }

        public void SendAction(string actionName)
        {
            Message actionM = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.createEntity);

            actionM.AddString(actionName);

            if (NetworkManager.instance.isOnServer)
            {
                NetworkManager.instance.server.SendToAll(actionM);
            }
            else
            {
                NetworkManager.instance.client.Send(actionM);
            }
        }



    }
}
