using System;
using System.Collections.Generic;
using System.Reflection;
using Game;
using General;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using UnityEngine.Events;
using Type = System.Type;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkHandler : MonoBehaviour
    {
        public delegate void ParamsAction(Dictionary<string, object> arguments);

        public static bool isOnServer;


        public PropertyInfo[] properties;
        public static List<Type> loadedTypes = new List<Type>();

        // first type in  key (Packet) second (Handler)
        public static Dictionary<Tuple<Type, Type>, UnityAction<Packet>> globalMethodBindings =
            new Dictionary<Tuple<Type, Type>, UnityAction<Packet>>();

        private static int count;
        public static Dictionary<int, NetworkHandler> bindRequests = new Dictionary<int, NetworkHandler>();


        public Component observed;

        

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
            isOnServer = Server.instance != null;
            NetworkManager.networkHandlers.Add(this);
            
            
            
            SetupLocalMarshalls();
        }




        

        public virtual void SetupLocalMarshalls()
        {
        }

        public void AddMethodMarshalling(Type packetType, UnityAction<Packet> process)
        {
            var handlerType = GetType();
            var t = new Tuple<Type, Type>(packetType, handlerType);
            if (!globalMethodBindings.ContainsKey(t))
                globalMethodBindings.Add(t, process);
        }


        public static void UnMarshall(Packet p)
        {
            var packetTypeString = p.Type;
            var packetType = Type.GetType(packetTypeString);
            var packetHandlerString = "com.mineorbit.dungeonsanddungeonscommon."+p.Handler;
            var networkHandlerType = Type.GetType(packetHandlerString);
            var handlerType = networkHandlerType;
            UnityAction<Packet> handle = null;
            while (!(globalMethodBindings.TryGetValue(new Tuple<Type, Type>(packetType, handlerType), out handle) ||
                     handlerType == typeof(NetworkHandler))) handlerType = handlerType.BaseType;
            if (handle != null)
            {
                handle.Invoke(p);
            }
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
        public void Marshall(int identity,IMessage message, bool TCP = true, bool overrideSame = false)
        {
            string[] fs = GetType().FullName.Split('.');
            var packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(message),
                Identity = identity
            };

            if (!isOnServer)
                NetworkManager.instance.client.WritePacket(packet, TCP);
            else
                Server.instance.WriteAll(packet, TCP);
        }
        

        // THIS IS FOR UNIDENTIFIED CALLS ONLY
        public static void Marshall(Type sendingHandler, IMessage message, bool TCP = true, bool overrideSame = false)
        {
            
            string[] fs = sendingHandler.FullName.Split('.');
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(message)
            };

            if (!isOnServer)
            {
                if (NetworkManager.instance != null)
                {
                    if (NetworkManager.instance.client != null)
                    {
                        NetworkManager.instance.client.WritePacket(packet, TCP);
                    }
                }
            }
            else
            {
                if(Server.instance != null)
                    Server.instance.WriteAll(packet, TCP);
            }
        }

        // THIS IS FOR UNIDENTIFIED CALLS ONLY
        public static void Marshall(Type sendingHandler, int identity, IMessage message, bool TCP = true, bool overrideSame = false)
        {
            string[] fs = sendingHandler.FullName.Split('.');
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(message),
                Identity = identity
            };

            if (!isOnServer)
                NetworkManager.instance.client.WritePacket(packet, TCP);
            else
                Server.instance.WriteAll(packet, TCP);
        }


        public static void Marshall(Type sendingHandler, IMessage message, int target, bool TCP = true, bool overrideSame = false)
        {
            string[] fs = sendingHandler.FullName.Split('.');
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(message)
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet, TCP);
            }
            else
            {
                if (Server.instance.clients[target] != null)
                    Server.instance.clients[target].WritePacket(packet, TCP);
            }
        }


        public static void Marshall(Type sendingHandler, IMessage message, int target, int identity, bool TCP = true, bool overrideSame = false)
        {
            string[] fs = sendingHandler.FullName.Split('.');
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(message),
                Identity = identity
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet);
            }
            else
            {
                if (Server.instance.clients[target] != null)
                    Server.instance.clients[target].WritePacket(packet, TCP);
            }
        }


        public static Vector3 toVector3(MVector v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Quaternion toQuaternion(MQuaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        
        public static MVector fromVector3(Vector3 v)
        {
            MVector mVector = new MVector();
            mVector.X = v.x;
            mVector.Y = v.y;
            mVector.Z = v.z;
            return mVector;
        }

        public static MQuaternion fromQuaternion(Quaternion q)
        {
            MQuaternion mQuaternion = new MQuaternion();
            mQuaternion.X = q.x;
            mQuaternion.Y = q.y;
            mQuaternion.Z = q.z;
            mQuaternion.W = q.w;
            return mQuaternion;
        }
        
        
        public void Marshall(int identity, IMessage message, int target, bool toOrWithout = true, bool TCP = true, bool overrideSame = false)
        {
            string[] fs = GetType().FullName.Split('.');
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(message),
                Identity = identity
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet, TCP,overrideSame);
            }
            else
            {
                if (toOrWithout)
                {
                    if (Server.instance.clients[target] != null)
                        Server.instance.clients[target].WritePacket(packet, TCP,overrideSame);
                }
                else
                {
                    Server.instance.WriteToAllExcept(packet, target, TCP,overrideSame);
                }
            }
        }

        [PacketBinding.Binding]
        public void SyncVarHandle(Packet value)
        {
            VarSync varSync;
            if (value.Content.TryUnpack(out varSync))
            {
                foreach (KeyValuePair<int,Any> kp in varSync.Content)
                {
                    PropertyInfo p = properties[kp.Key];
                    if (p.PropertyType.ToString() == "UnityEngine.Vector3")
                    {
                        Vector3 v = toVector3(kp.Value.Unpack<MVector>());
                        p.SetValue(this,v);
                    }
                }
            }
        }
    }
}
