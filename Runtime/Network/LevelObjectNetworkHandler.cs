using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General;
using Game;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectNetworkHandler : NetworkHandler
    {
        public new LevelObject observed;

        List<string> availableActions = new List<string>();

        public virtual void Awake()
        {
            observed = GetComponent<LevelObject>();
            base.Awake();
        }

        // CONNECT HERE BASED ON POSITION
        public virtual void Start()
        {
            if(isOnServer)
            ConnectLevelObject();
        }

        public void ConnectLevelObject()
        {
            Game.ConnectLevelObject connectLevelObject = new ConnectLevelObject
            {
                Identity = this.Identity,
                X = transform.position.x,
                Y = transform.position.y,
                Z = transform.position.z,
                HandlerType = this.GetType().FullName
            };
            Marshall(this.GetType(),connectLevelObject);
        }

        public static void RequestBind(string failedBindIdentity)
        {
            ConnectLevelObjectRequest connectLevelObjectRequest = new ConnectLevelObjectRequest
            {
                Identity = failedBindIdentity
            };
            Debug.Log("Asking for Bind of "+failedBindIdentity);
            Marshall(typeof(LevelObjectNetworkHandler),connectLevelObjectRequest,failedBindIdentity);
        }

        [PacketBinding.Binding]
        public void OnConnectLevelObjectRequest(Packet p)
        {
            ConnectLevelObjectRequest connectLevelObjectRequest;
            if(p.Content.TryUnpack<ConnectLevelObjectRequest>(out connectLevelObjectRequest))
            {
                MainCaller.Do(() =>
                {
                    Debug.Log("Processing Rebind Request");
                    ConnectLevelObject();
                });
            }
        }


        [PacketBinding.Binding]
        public static void OnConnectLevelObject(Packet p)
        {
            MainCaller.Do(() => {

                Debug.Log("Handling");
                float eps = 0.5f;
                Game.ConnectLevelObject levelObjectConnect;
                if (p.Content.TryUnpack<Game.ConnectLevelObject>(out levelObjectConnect))
                {
                    Vector3 handlerPosition = new Vector3(levelObjectConnect.X, levelObjectConnect.Y, levelObjectConnect.Z);
                    Type handlerType = Type.GetType(levelObjectConnect.HandlerType);
                    NetworkHandler fittingHandler = NetworkManager.networkHandlers.Find((x) => {
                        float distance = (handlerPosition - x.transform.position).magnitude;
                        Debug.Log(x+" "+distance);
                        return x.GetType() == handlerType && distance < eps;
                    });
                    if (fittingHandler != null)
                    {
                        fittingHandler.Identity = levelObjectConnect.Identity;
                    }
                    else
                    {
                        Debug.Log("No " + handlerType + " found at " + handlerPosition);
                        RequestBind(levelObjectConnect.Identity);
                    }
                }

            });

            
        }


        // THIS NEEDS TO UNPACK ANY INTO ARGUMENTS FOR DYNAMIC CALL
        [PacketBinding.Binding]
        public void ProcessAction(Packet p)
        {

        }

        public void SendAction(string actionName)
        {
            if(availableActions.Contains(actionName))
            {
                LevelObjectAction action = new LevelObjectAction
                {
                    ActionName =  actionName
                };
                Marshall(action);
            }
        }


        public class ActionParam
        {
            internal static ActionParam From<T>(T argument)
            {
                throw new NotImplementedException();
            }
            public  (string,Google.Protobuf.WellKnownTypes.Any) Pack()
            {
                return (null,null);
            }
        }

        // NOT COMPLETED
        // THIS NEEDS TO PACK ARGUMENTS INTO ANY
        public void SendAction(string actionName, ActionParam argument)
        {
            if (availableActions.Contains(actionName))
            {
                LevelObjectAction action = new LevelObjectAction
                {
                    ActionName = actionName
                };
                Dictionary<string, Google.Protobuf.WellKnownTypes.Any> arguments = new Dictionary<string, Google.Protobuf.WellKnownTypes.Any>();

                var packedArgument = argument.Pack();
                
                arguments.Add(packedArgument.Item1,packedArgument.Item2);

                action.Params.Add(arguments);
                Marshall(action);
            }
        }


        // Not yet needed
        /*
        public void SendAction(string actionName, object param)
        {
            if (availableActions.Contains(actionName))
            {

            }
        }
        */
    }
}
