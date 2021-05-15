using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General;
using Game;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Linq;

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





        // CALLABLE METHODS MUST BE MARKED PUBLIC TO BE USABLE
        [PacketBinding.Binding]
        public void ProcessAction(Packet p)
        {
            LevelObjectAction levelObjectAction;
            if (p.Content.TryUnpack<LevelObjectAction>(out levelObjectAction))
            {
                MethodInfo methodInfo = observed.GetType().GetMethod(levelObjectAction.ActionName);

                List<object> parameters = new List<object>();
                foreach(KeyValuePair<int,Parameter> k in levelObjectAction.Params)
                {
                    Debug.Log("Parameter: "+k.Key+" "+k.Value.Type);
                    ActionParam actionParam = ActionParam.Unpack(new Tuple<int, Parameter>(k.Key,k.Value));
                    parameters.Add(actionParam.data);
                }
                object[] paramObjects = new object[levelObjectAction.Params.Count];
                if(methodInfo != null)
                { 
                    MainCaller.Do(() => {
                    methodInfo.Invoke(observed,parameters.ToArray()); });
                }else
                {
                    Debug.Log("The Method with the given name "+levelObjectAction.ActionName+" could not be found, is it private?");
                }
            }
        }



        public class ActionParam
        {
            public int FieldPlace;
            public Type type;
            public object data;
            internal static ActionParam From<T>(T argument, int fieldPlace = 0)
            {
                ActionParam actionParam = new ActionParam();
                actionParam.FieldPlace = fieldPlace;
                actionParam.type = argument.GetType();
                actionParam.data = argument;
                return actionParam;
            }


            public static ActionParam Unpack(Tuple<int,Parameter> data)
            {
                ActionParam actionParam = new ActionParam();
                actionParam.FieldPlace = data.Item1;

                actionParam.type = Type.GetType(data.Item2.Type);

                if (actionParam.type == typeof(ChunkData))
                {

                    NetLevel.ChunkData netChunkData = data.Item2.Value.Unpack<NetLevel.ChunkData>();
                    actionParam.data = ChunkData.FromNetData(netChunkData);
                }
                else
                if(actionParam.type == typeof(bool))
                {
                    actionParam.data = data.Item2.Value.Unpack<Google.Protobuf.WellKnownTypes.BoolValue>().Value;
                }else
                if(actionParam.type.IsSubclassOf( typeof(NetworkLevelObject)) || actionParam.type == typeof(NetworkLevelObject))
                {
                    string identityOfParam = data.Item2.Value.Unpack<Google.Protobuf.WellKnownTypes.StringValue>().Value;

                    LevelObjectNetworkHandler h = NetworkHandler.FindByIdentity<LevelObjectNetworkHandler>(identityOfParam);

                    actionParam.data = h.observed;
                }


                return actionParam;
            }


            public (int, Parameter) Pack()
            {

                Google.Protobuf.WellKnownTypes.Any x = null;


                if (type == typeof(ChunkData))
                {
                    NetLevel.ChunkData chunkData = ChunkData.ToNetData((ChunkData) data);
                    x = Google.Protobuf.WellKnownTypes.Any.Pack(chunkData);
                }
                else 
                if (type == typeof(bool))
                {
                    Google.Protobuf.WellKnownTypes.BoolValue bv = new Google.Protobuf.WellKnownTypes.BoolValue();
                    bv.Value = (bool) data;
                    x = Google.Protobuf.WellKnownTypes.Any.Pack(bv);
                }
                else
                if (type.IsSubclassOf(typeof(NetworkLevelObject)) || type == typeof(NetworkLevelObject))
                {
                    LevelObjectNetworkHandler h = ((NetworkLevelObject)data).levelObjectNetworkHandler;
                    Google.Protobuf.WellKnownTypes.StringValue sv = new Google.Protobuf.WellKnownTypes.StringValue();
                    sv.Value = h.Identity;
                    x = Google.Protobuf.WellKnownTypes.Any.Pack(sv);
                }

                Parameter p = new Parameter
                {
                    Type = type.FullName,
                    Value = x

                };
                return (FieldPlace, p);
            }
        }
          

        // THIS NEEDS A SAFETY LIST LATER

        // NOT COMPLETED
        // THIS NEEDS TO PACK ARGUMENTS INTO ANY
        public virtual void SendAction(string actionName, ActionParam argument)
        {
                LevelObjectAction action = new LevelObjectAction
                {
                    ActionName = actionName
                };
                Dictionary<int, Parameter> arguments = new Dictionary<int, Parameter>();

                var packedArgument = argument.Pack();
                
                arguments.Add(packedArgument.Item1,packedArgument.Item2);

                action.Params.Add(arguments);
                Marshall(action);
        }


        public void SendAction(string actionName)
        {
            LevelObjectAction action = new LevelObjectAction
            {
                ActionName = actionName
            };
            Marshall(action);

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
