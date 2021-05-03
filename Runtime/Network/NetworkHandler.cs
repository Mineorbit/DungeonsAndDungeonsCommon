using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;
using General;
using Google.Protobuf;
using State;

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
        public static Dictionary<Tuple<Type,Type>, Action<Packet>> globalMethodMarshallings = new Dictionary<Tuple<Type, Type>, Action<Packet>>();
        


        //Fetch Methods
        public virtual void Awake()
        {
            networkHandlers.Add(this);

            AddMethodMarshalling(typeof(LevelObjectConnect),ConnectHandler);
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

        


        public void Marshall(string methodName)
        {
        }


        public void Marshall(string methodName, object param)
        {
        }

    }
}