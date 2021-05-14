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

        public string _Identity;

        public string Identity
        {
            get
            {
                return _Identity;
            }
            set
            {
                identified = true;
                _Identity = value;
            }
        }

        public bool identified;

        public Component observed;

        public static bool isOnServer;

        public delegate void ParamsAction(Dictionary<string, object> arguments);




        public static List<Type> loadedTypes = new List<Type>();

        // first type in  key (Packet) second (Handler)
        public static Dictionary<Tuple<Type, Type>, UnityAction<Packet>> globalMethodBindings = new Dictionary<Tuple<Type, Type>, UnityAction<Packet>>();


        //Fetch Methods
        public virtual void Awake()
        {
            this.enabled = NetworkManager.isConnected;
                if (!this.enabled) return;
            isOnServer = Server.instance != null;
            NetworkManager.networkHandlers.Add(this);

            SetupLocalMarshalls();
            if(isOnServer)
            Identity = GetInstanceID().ToString();


        }


        public virtual void Start()
        {
            if (!isOnServer)
                StartRequestBind(this);


            this.enabled = NetworkManager.isConnected;
        }




        public void ConnectLevelObject(int rn)
        {
            Game.ConnectLevelObject connectLevelObject = new ConnectLevelObject
            {
                Identity = this.Identity,
                X = transform.position.x,
                Y = transform.position.y,
                Z = transform.position.z,
                HandlerType = this.GetType().FullName,
                ResponseNumber = rn
            };
            Marshall(this.GetType(), connectLevelObject);
        }


        static int count = 0;
        public static Dictionary<int, NetworkHandler> bindRequests = new Dictionary<int, NetworkHandler>();
        public static void StartRequestBind(NetworkHandler handler)
        {
            ConnectLevelObjectRequest connectLevelObjectRequest = null;
            NetworkHandler parentNetworkHandler = handler.transform.GetComponentInParent<NetworkHandler>();
            if (parentNetworkHandler != null && parentNetworkHandler.identified)
            {
                Debug.Log("Connecting "+handler.gameObject.name+" via parent");
                connectLevelObjectRequest = new ConnectLevelObjectRequest
                {
                    ParentIdentity = parentNetworkHandler.Identity,
                    HandlerType = handler.GetType().FullName,
                    RequestNumber = count,
                    RequestType = ConnectLevelObjectRequest.Types.RequestType.ByParent
                };
            }
            else if (parentNetworkHandler == null)
            {
                connectLevelObjectRequest = new ConnectLevelObjectRequest
                {
                    X = handler.transform.position.x,
                    Y = handler.transform.position.y,
                    Z = handler.transform.position.z,
                    HandlerType = handler.GetType().FullName,
                    RequestNumber = count,
                    RequestType = ConnectLevelObjectRequest.Types.RequestType.ByPosition
                };
            }
            else
            {
                handler.Invoke("TryAfter", 2f);
                return;
            }


            bindRequests.Add(count, handler);
            count++;
            Marshall(typeof(LevelObjectNetworkHandler), connectLevelObjectRequest);
        }


        void TryAfter()
        {
            StartRequestBind(this);
        }


        [PacketBinding.Binding]
        public static void OnConnectLevelObject(Packet p)
        {
            Game.ConnectLevelObject connectLevelObjectResponse;
            if (p.Content.TryUnpack<Game.ConnectLevelObject>(out connectLevelObjectResponse))
            {
                NetworkHandler bindedHandler;
                if (bindRequests.TryGetValue(connectLevelObjectResponse.ResponseNumber, out bindedHandler))
                {

                    MainCaller.Do(() =>
                    {
                        Debug.Log("Processing Rebind Response");
                        bindedHandler.Identity = connectLevelObjectResponse.Identity;
                        bindRequests.Remove(connectLevelObjectResponse.ResponseNumber);
                    });
                }

            }
        }

        [PacketBinding.Binding]
        public static void OnConnectLevelObjectRequestFail(Packet p)
        {
            ConnectLevelObjectFail connectLevelObjectFail;
            if (p.Content.TryUnpack<ConnectLevelObjectFail>(out connectLevelObjectFail))
            {
                NetworkHandler networkHandler;
                if (bindRequests.TryGetValue(connectLevelObjectFail.ResponseNumber, out networkHandler))
                {
                    MainCaller.Do(() => { StartRequestBind(networkHandler); });
                }
            }
        }


        [PacketBinding.Binding]
        public static void OnConnectLevelObjectRequest(Packet p)
        {
            MainCaller.Do(() => {

                Debug.Log("Handling");
                float eps = 0.25f;
                Game.ConnectLevelObjectRequest levelObjectConnect;
                if (p.Content.TryUnpack<Game.ConnectLevelObjectRequest>(out levelObjectConnect))
                {
                    NetworkHandler fittingHandler = null;
                    Type handlerType = Type.GetType(levelObjectConnect.HandlerType);
                    if (levelObjectConnect.RequestType == ConnectLevelObjectRequest.Types.RequestType.ByPosition)
                    {

                        Vector3 handlerPosition = new Vector3(levelObjectConnect.X, levelObjectConnect.Y, levelObjectConnect.Z);
                        fittingHandler = (NetworkHandler)NetworkManager.networkHandlers.Find((x) => {
                            float distance = (handlerPosition - x.transform.position).magnitude;
                            return x.GetType() == handlerType && distance < eps && !x.identified;
                        });


                    }
                    else if (levelObjectConnect.RequestType == ConnectLevelObjectRequest.Types.RequestType.ByParent)
                    {
                        fittingHandler = (NetworkHandler)NetworkManager.networkHandlers.Find((x) => {
                            NetworkHandler p = x.GetComponentInParent<NetworkHandler>();
                            return x.GetType() == handlerType && p.identified && p.Identity
                            == levelObjectConnect.ParentIdentity && !x.identified;
                        });
                    }


                    if (fittingHandler != null)
                    {
                        fittingHandler.ConnectLevelObject(levelObjectConnect.RequestNumber);
                    }
                    else
                    {
                        Debug.Log("No " + handlerType + " found");

                        ConnectLevelObjectFail connectLevelObjectFail = new ConnectLevelObjectFail
                        {
                            HandlerType = levelObjectConnect.HandlerType,
                            ResponseNumber = levelObjectConnect.RequestNumber
                        };

                        //THIS NEEDS TO BE FOUND IN PACKET

                        int requester = p.Sender;
                        Marshall(typeof(LevelObjectNetworkHandler), connectLevelObjectFail, requester);

                    }
                }

            });


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

                handlerType = handlerType.BaseType;
                }
                if(handle != null)
                {
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
        public void Marshall(IMessage message, bool TCP = true)
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
                NetworkManager.instance.client.WritePacket(packet, TCP: TCP);
            }else
            {
                Server.instance.WriteAll(packet,TCP: TCP);
            }
        }



        // THIS IS FOR UNIDENTIFIED CALLS ONLY
        public static void Marshall(Type sendingHandler,IMessage message, bool TCP = true)
        {
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = sendingHandler.FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message)
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet, TCP: TCP);
            }
            else
            {
                Server.instance.WriteAll(packet, TCP: TCP);
            }
        }

        // THIS IS FOR UNIDENTIFIED CALLS ONLY
        public static void Marshall(Type sendingHandler, IMessage message, string identity, bool TCP = true)
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
                NetworkManager.instance.client.WritePacket(packet, TCP: TCP);
            }
            else
            {
                Server.instance.WriteAll(packet, TCP: TCP);
            }
        }


        public static void Marshall(Type sendingHandler, IMessage message, int target, bool TCP = true)
        {
            Packet packet = new Packet
            {
                Type = message.GetType().FullName,
                Handler = sendingHandler.FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message)
            };

            if (!isOnServer)
            {
                NetworkManager.instance.client.WritePacket(packet, TCP: TCP);
            }
            else
            {
                if (Server.instance.clients[target] != null)
                    Server.instance.clients[target].WritePacket(packet, TCP: TCP);
            }
        }


        public static void Marshall(Type sendingHandler, IMessage message, int target, string identity, bool TCP = true)
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
                    Server.instance.clients[target].WritePacket(packet, TCP: TCP);
            }
        }


        public void Marshall(IMessage message, int target, bool toOrWithout = true, bool TCP  = true)
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
                NetworkManager.instance.client.WritePacket(packet, TCP: TCP);
            }
            else
            {
                if(toOrWithout)
                { 
                if(Server.instance.clients[target] != null)
                Server.instance.clients[target].WritePacket(packet, TCP: TCP);
                }else
                {
                    Server.instance.WriteAll(packet,target, TCP: TCP);
                }
            }
        }

    }
}