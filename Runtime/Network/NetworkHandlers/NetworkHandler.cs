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
        public delegate void ParamsAction(Dictionary<string, object> arguments);
        

        public PropertyInfo[] properties;
        public static List<Type> loadedTypes = new List<Type>();

        // first type in  key (Packet) second (Handler)
       

        private static int count;
        public static Dictionary<int, NetworkHandler> bindRequests = new Dictionary<int, NetworkHandler>();


        public Component observed;



        public static LevelObjectNetworkHandler ByIdentity(int identity)
        {
            return (LevelObjectNetworkHandler) NetworkManager.networkHandlers.Find((x) =>
            {
                if (x.GetType().IsSubclassOf(typeof(LevelObjectNetworkHandler)) &&
                    ((NetworkLevelObject) x.observed).Identity == identity)
                {
                    return true;
                }

                return false;
            });
        }
        
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

            if (!enabled) return;
            
            NetworkManager.networkHandlers.Add(this);
            
            
            
        }



        public void FixedUpdate()
        {
            UpdateSyncVar();
        }

        public void UpdateSyncVar()
        {
            if(NetworkManager.instance.isOnServer && properties != null && properties.Length>0)
            {
                Message actionM = Message.Create(MessageSendMode.unreliable, (ushort) NetworkManager.ServerToClientId.syncVar);
                actionM.AddInt(((NetworkLevelObject) observed).Identity);
                foreach (var info in properties)
                {
                    if (info.PropertyType.FullName == "System.Boolean")
                    {
                        actionM.AddBool((bool)info.GetValue(this));
                    }else
                    if (info.PropertyType.FullName == "UnityEngine.Vector3")
                    {
                        actionM.AddVector3((Vector3)info.GetValue(this));
                    }else
                    if (info.PropertyType.FullName == "UnityEngine.Quaternion")
                    {
                        actionM.AddQuaternion((Quaternion)info.GetValue(this));
                    }else
                    if (info.PropertyType.FullName == "System.Int32")
                    {
                        actionM.AddInt((int)info.GetValue(this));
                    }
                    else
                    {
                        GameConsole.Log($"Could not add property {info.PropertyType.Name} to package");
                    }
                    NetworkManager.instance.Server.SendToAll(actionM);
                }
            
            }
        }



        [MessageHandler((ushort)NetworkManager.ServerToClientId.syncVar)]
        public static void SyncVarHandle(Message value)
        {
            int identity = value.GetInt();
            NetworkHandler n = NetworkHandler.ByIdentity(identity);
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
                GameConsole.Log($"Could not find {identity} for syncvar");
            }
        }
    }
}
