using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;
using General;
using Google.Protobuf;
using State;
using Game;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkHandler : MonoBehaviour
    {
        public string Identity;

        public Component observed;

        public static bool isOnServer;

        public delegate void ParamsAction(Dictionary<string, object> arguments);




        public static List<Type> loadedTypes = new List<Type>();
        // first type in  key (Packet) second (Handler)
        public static Dictionary<Tuple<Type, Type>, UnityAction<Packet>> globalMethodBindings = new Dictionary<Tuple<Type, Type>, UnityAction<Packet>>();


        //Fetch Methods
        public virtual void Awake()
        {
            isOnServer = Server.instance != null;
            NetworkManager.networkHandlers.Add(this);

            SetupLocalMarshalls();
            if(isOnServer)
            Identity = GetInstanceID().ToString();


        }

        public virtual void SetupLocalMarshalls()
        {
        }

        public void AddMethodMarshalling(Type packetType, UnityAction<Packet> process)
        {
            Type handlerType = this.GetType();
            Tuple<Type, Type> t = new Tuple<Type, Type>(packetType, handlerType);
            if(!globalMethodBindings.ContainsKey(t))
            globalMethodBindings.Add(t,process);

        }


        public static void UnMarshall(Packet p)
        {
            string packetTypeString = p.Type;
            Type packetType = Type.GetType(packetTypeString);
            string packetHandlerString = p.Handler;
            Type networkHandlerType = Type.GetType(packetHandlerString);



                Type handlerType = networkHandlerType;
                UnityAction<Packet> handle = null;
                while (! (globalMethodBindings.TryGetValue(new Tuple<Type, Type>(packetType,handlerType), out handle) || (handlerType == typeof(NetworkHandler))))
                {

                Debug.Log("Did not find Binding in "+handlerType);
                handlerType = handlerType.BaseType;
                Debug.Log("Elevating to " + handlerType);
                }
                if(handle != null)
                {
                Debug.Log("Handle found " + handle);
                handle.Invoke(p);
                }
        }

        public static T FindByIdentity<T>(string identity) where T : NetworkHandler
        {
            return (T) NetworkManager.networkHandlers.Find((x) => x.Identity == identity); 
        }


        /*
        public void AddMethodMarshalling(Action<Dictionary<string, object>> a, string name = "")
        {
            string actionName = name;
            if(name == "")
            {
                actionName = a.Method.Name;
            }
            methods.Add(actionName,a);
        }
        */



        

        // eventually type strings are not jet correcty matched
        public void Marshall(IMessage message)
        {
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = this.GetType().FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message),
                Identity = this.Identity
            };

            if(!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet);
            }else
            {
                Server.instance.WriteAll(packet);
            }
        }



        // THIS IS FOR UNIDENTIFIED CALLS ONLY
        public static void Marshall(Type sendingHandler,IMessage message)
        {
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = sendingHandler.FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message)
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet);
            }
            else
            {
                Server.instance.WriteAll(packet);
            }
        }

        // THIS IS FOR UNIDENTIFIED CALLS ONLY
        public static void Marshall(Type sendingHandler, IMessage message, string identity)
        {
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = sendingHandler.FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message),
                Identity = identity
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet);
            }
            else
            {
                Server.instance.WriteAll(packet);
            }
        }



        public static void Marshall(Type sendingHandler, IMessage message, int target, string identity)
        {
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = sendingHandler.FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message),
                Identity = identity
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet);
            }
            else
            {
                if (Server.instance.clients[target] != null)
                    Server.instance.clients[target].WritePacket(packet);
            }
        }


        public void Marshall(IMessage message, int target)
        {
            Packet packet = new Packet
            {
                Type = message.GetType().ToString(),
                Handler = this.GetType().ToString(),
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message),
                Identity = this.Identity
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet);
            }
            else
            {
                if(Server.instance.clients[target] != null)
                Server.instance.clients[target].WritePacket(packet);
            }
        }

    }
}