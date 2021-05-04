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

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkHandler : MonoBehaviour
    {
        public string Identity;

        public Component observed;

        public static bool isOnServer;

        public delegate void ParamsAction(Dictionary<string, object> arguments);



        public static List<NetworkHandler> networkHandlers = new List<NetworkHandler>();

        public static List<Type> loadedTypes = new List<Type>();
        // first type in  key (Packet) second (Handler)
        public static Dictionary<Tuple<Type,Type>, Action<Packet>> globalMethodMarshallings = new Dictionary<Tuple<Type, Type>, Action<Packet>>
        {
            {new Tuple<Type, Type>(typeof(LevelObjectConnect),typeof(NetworkHandler)), ConnectHandler},
            {new Tuple<Type, Type>(typeof(PlayerCreate),typeof(PlayerNetworkHandler)), (x) => { PlayerNetworkHandler.HandleCreatePacket(x); } }
        };
        


        //Fetch Methods
        public virtual void Awake()
        {
            isOnServer = Server.instance != null;
            networkHandlers.Add(this);
            SetupLocalMarshalls();

        }

        public virtual void SetupLocalMarshalls()
        {
        }

        public void AddMethodMarshalling(Type packetType, Action<Packet> process)
        {
            Type handlerType = this.GetType();
            Tuple<Type, Type> t = new Tuple<Type, Type>(packetType, handlerType);
            if(!globalMethodMarshallings.ContainsKey(t))
            globalMethodMarshallings.Add(t,process);

        }


        public static void UnMarshall(Packet p)
        {
            string packetTypeString = p.Type;
            Type packetType = Type.GetType(packetTypeString);
            string packetHandlerString = p.Handler;
            Type networkHandlerType = Type.GetType(packetHandlerString);

            Debug.Log("Handling Packet "+packetTypeString+" "+packetHandlerString);

            Debug.Log("Types: "+ packetType+" "+networkHandlerType);


                Action<Packet> handle;
                if (globalMethodMarshallings.TryGetValue(new Tuple<Type, Type>(packetType,networkHandlerType), out handle))
                {
                    handle.Invoke(p);
                }else
                {
                    Debug.Log("No Global Handle for " + packetType + " in " + networkHandlerType + " was found");
                }
        }

        public static T FindByIdentity<T>(string identity) where T : NetworkHandler
        {
            return (T) networkHandlers.Find((x) => x.Identity == identity); 
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


        // works only for static objects
        // Add type check eventually
        private static void ConnectHandler(Packet connectPackage)
        {
            float eps = 0.0005f;
            LevelObjectConnect levelObjectConnect;
            if(connectPackage.Content.TryUnpack<LevelObjectConnect>(out levelObjectConnect))
            {
                Vector3 handlerPosition = new Vector3(levelObjectConnect.X,levelObjectConnect.Y,levelObjectConnect.Z);
                foreach(NetworkHandler h in networkHandlers)
                {
                    float distance = (handlerPosition - h.transform.position).magnitude;
                    if(distance < eps)
                    {
                        h.Identity = levelObjectConnect.Identity;

                        return;
                    }
                }
            }
        }

        

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

            if(isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet);
            }else
            {
                Server.instance.WriteAll(packet);
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

            if (isOnServer)
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