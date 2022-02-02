using System;
using System.Collections.Generic;
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
            NetworkManager.networkHandlers.Find((x) =>
            {
                if (x.GetType().IsSubclassOf(typeof(LevelObjectNetworkHandler)) &&
                    ((NetworkLevelObject) x.observed).Identity == identity)
                {
                    return true;
                }

                return false;
            });
            return null;
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
            
            if (!enabled) return;
            NetworkManager.networkHandlers.Add(this);
            
            
            
        }







        [MessageHandler((ushort)NetworkManager.ServerToClientId.syncVar)]
        public void SyncVarHandle(Message value)
        {

            foreach (var info in properties)
            {
                object v = null;
                if (info.PropertyType.IsSubclassOf(typeof(bool)))
                {
                    v = value.GetBool();
                }
                if (info.PropertyType.IsSubclassOf(typeof(Vector3)))
                {
                    v = value.GetVector3();
                }
                if (info.PropertyType.IsSubclassOf(typeof(Quaternion)))
                {
                    v = value.GetQuaternion();
                }
                
                info.SetValue(this,v);
            }
            
            
        }
    }
}
