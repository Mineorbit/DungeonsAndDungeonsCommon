using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General;
using Game;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectNetworkHandler : NetworkHandler
    {
        public new NetworkLevelObject observed;

        List<string> availableActions = new List<string>();



        public virtual void Awake()
        {
            observed = GetComponent<NetworkLevelObject>();
            base.Awake();

            // currently not updated
            if (observed != null)
                observed.enabled = (!NetworkManager.isConnected || isOnServer);
        }

        // CONNECT HERE BASED ON POSITION
        public virtual void Start()
        {
            if (!isOnServer)
                StartRequestBind();
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
            Marshall(this.GetType(),connectLevelObject);
        }


        static int count = 0;
        public static Dictionary<int, LevelObjectNetworkHandler> bindRequests = new Dictionary<int, LevelObjectNetworkHandler>();
        public void StartRequestBind()
        {
            ConnectLevelObjectRequest connectLevelObjectRequest = new ConnectLevelObjectRequest
            {
                X = transform.position.x,
                Y = transform.position.y,
                Z = transform.position.z,
                HandlerType = this.GetType().FullName,
                RequestNumber = count
            };

            bindRequests.Add(count,this);
            count++;
            Marshall(typeof(LevelObjectNetworkHandler), connectLevelObjectRequest);
        }


        [PacketBinding.Binding]
        public static void OnConnectLevelObject(Packet p)
        {
            Game.ConnectLevelObject connectLevelObjectResponse;
            if(p.Content.TryUnpack<Game.ConnectLevelObject>(out connectLevelObjectResponse))
            {
                LevelObjectNetworkHandler bindedHandler;
                if(bindRequests.TryGetValue(connectLevelObjectResponse.ResponseNumber,out bindedHandler))
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
        public static void OnConnectLevelObjectRequest(Packet p)
        {
            MainCaller.Do(() => {

                Debug.Log("Handling");
                float eps = 0.025f;
                Game.ConnectLevelObjectRequest levelObjectConnect;
                if (p.Content.TryUnpack<Game.ConnectLevelObjectRequest>(out levelObjectConnect))
                {
                    Vector3 handlerPosition = new Vector3(levelObjectConnect.X, levelObjectConnect.Y, levelObjectConnect.Z);
                    Type handlerType = Type.GetType(levelObjectConnect.HandlerType);
                    LevelObjectNetworkHandler fittingHandler = (LevelObjectNetworkHandler) NetworkManager.networkHandlers.Find((x) => {
                        float distance = (handlerPosition - x.transform.position).magnitude;
                        return x.GetType() == handlerType && distance < eps;
                    });
                    if (fittingHandler != null)
                    {
                        fittingHandler.ConnectLevelObject(levelObjectConnect.RequestNumber);
                    }
                    else
                    {
                        Debug.Log("No " + handlerType + " found at " + handlerPosition);
                    }
                }

            });

            
        }


        // THIS NEEDS TO UNPACK ANY INTO ARGUMENTS FOR DYNAMIC CALL
        [PacketBinding.Binding]
        public void ProcessAction(Packet p)
        {
            LevelObjectAction levelObjectAction;
            if (p.Content.TryUnpack<LevelObjectAction>(out levelObjectAction))
            {
            
            }
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
            public Type type;
            public object data;
            internal static ActionParam From<T>(T argument)
            {
                ActionParam actionParam = new ActionParam();
                actionParam.type = argument.GetType();
                actionParam.data = argument;
                return actionParam;
            }
                public (string, Google.Protobuf.WellKnownTypes.Any) Pack()
                {

                Google.Protobuf.WellKnownTypes.Any x = null;
                if (type == typeof(ChunkData))
                {
                    ChunkData inData = (ChunkData)data;

                    MemoryStream memoryStream = new MemoryStream();
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(memoryStream, inData);
                    NetLevel.ChunkData chunkData = new NetLevel.ChunkData
                    {
                        ChunkId = inData.chunkId,
                        Data = Google.Protobuf.ByteString.FromStream(memoryStream)
                    };
                    x = Google.Protobuf.WellKnownTypes.Any.Pack(chunkData);
                }

                return (type.FullName, x);
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
