using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.Events;
using Type = System.Type;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkHandler : MonoBehaviour
    {
        
        public PropertyInfo[] properties;
        
        // first type in  key (Packet) second (Handler)
        
        private static int count;

        public Component observed;


        public bool[] existsOn = new[] {true, true, true, true};
        
        public virtual void OnDestroy()
        {
            NetworkManager.networkHandlers.Remove(this);
        }

        //Fetch Methods
        public virtual void Awake()
        {

            enabled = Level.instantiateType == Level.InstantiateType.Play ||
                      Level.instantiateType == Level.InstantiateType.Default ||
                      Level.instantiateType == Level.InstantiateType.Online;
            
            properties = this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PacketBinding.SyncVar))).ToArray();

            NetworkManager.networkHandlers.Add(this);
            if (!enabled) return;
            
            if (observed.GetType().IsSubclassOf(typeof(Entity)))
            {
                // Interactive LevelObjects and other things always exist
                existsOn = new[] {false, false, false, false};
            }

        }


        public virtual void ResetHandler()
        {
            
        }


        public virtual void FixedUpdate()
        {
            if(NetworkManager.instance.isOnServer)
            {
                UpdateSyncVar();
            }
        }

        public void UpdateSyncVar()
        {
            if(NetworkManager.instance.isOnServer && properties != null && properties.Length>0)
            {
                Message syncVarMessage = Message.Create(MessageSendMode.unreliable, (ushort) NetworkManager.ServerToClientId.syncVar);
                syncVarMessage.AddInt(((NetworkLevelObject) observed).Identity);
                foreach (var info in properties)
                {
                    if (info.PropertyType.FullName == "System.Boolean")
                    {
                        syncVarMessage.AddBool((bool)info.GetValue(this));
                    }else
                    if (info.PropertyType.FullName == "UnityEngine.Vector3")
                    {
                        syncVarMessage.AddVector3((Vector3)info.GetValue(this));
                    }else
                    if (info.PropertyType.FullName == "UnityEngine.Quaternion")
                    {
                        syncVarMessage.AddQuaternion((Quaternion)info.GetValue(this));
                    }else
                    if (info.PropertyType.FullName == "System.Int32")
                    {
                        syncVarMessage.AddInt((int)info.GetValue(this));
                    }
                    else
                    {
                        GameConsole.Log($"Could not add property {info.PropertyType.Name} to package");
                    }
                }
                
                SendToExisting(syncVarMessage);
            
            }
        }

        public void SendToExisting(Message m)
        {
            for (int i = 0;i < 4;i++)
            {
                if (existsOn[i])
                {
                    NetworkManager.instance.Server.Send(m,(ushort)(i+1));
                }
            }

        }


        [MessageHandler((ushort)NetworkManager.ServerToClientId.syncVar)]
        public static void SyncVarHandle(Message value)
        {
            int identity = value.GetInt();
            NetworkLevelObject o = NetworkLevelObject.FindByIdentity(identity);

            if (o == null)
            {
                GameConsole.Log($"Could not find LevelObject {identity}");
                return;
            }
            
            NetworkHandler n =  o.levelObjectNetworkHandler;
            try
            {
                if(n != null)
                {
                foreach (var info in n.properties)
                {
                    object v = null;
                if (info.PropertyType.FullName == "System.Boolean")
                {
                    v = value.GetBool();
                }else
                if (info.PropertyType.FullName == "UnityEngine.Vector3")
                {
                    v = value.GetVector3();
                }else
                if (info.PropertyType.FullName == "UnityEngine.Quaternion")
                {
                    v = value.GetQuaternion();
                }else
                if (info.PropertyType.FullName == "System.Int32")
                {
                    v = value.GetInt();
                }
                else
                {
                    GameConsole.Log($"Could not parse value of {info} from varsync");
                }
                
                info.SetValue(n,v);
                }
                
            }
            else
            {
               // GameConsole.Log($"Could not find {identity} for syncvar");
            }
            }
            catch (Exception e)
            {
                GameConsole.Log($"SyncVar: {n.gameObject} {identity}: {e} ");
                throw;
            }
        }
    }
}
