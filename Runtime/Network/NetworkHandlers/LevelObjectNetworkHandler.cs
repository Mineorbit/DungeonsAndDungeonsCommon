using System;
using System.Collections.Generic;
using Game;
using General;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using Type = System.Type;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelObjectNetworkHandler : NetworkHandler
    {
        public new NetworkLevelObject observed;

        private List<string> availableActions = new List<string>();

        public bool disabled_observed = true;
        public virtual void Awake()
        {
            observed = GetComponent<NetworkLevelObject>();
            base.Awake();
            
            // currently not updated
            if (observed != null)
                observed.enabled = !disabled_observed || (!NetworkManager.isConnected || isOnServer);
        }


        // CALLABLE METHODS MUST BE MARKED PUBLIC TO BE USABLE
        [PacketBinding.Binding]
        public virtual void ProcessAction(Packet p)
        {
            LevelObjectAction levelObjectAction;
            if (p.Content.TryUnpack(out levelObjectAction))
            {
                var methodInfo = observed.GetType().GetMethod(levelObjectAction.ActionName);

                var parameters = new List<object>();
                foreach (var k in levelObjectAction.Params)
                {
                    Debug.Log("Parameter: " + k.Key + " " + k.Value.Type);
                    var actionParam = ActionParam.Unpack(new Tuple<int, Parameter>(k.Key, k.Value));
                    parameters.Add(actionParam.data);
                }

                var paramObjects = new object[levelObjectAction.Params.Count];
                if (methodInfo != null)
                    MainCaller.Do(() => { methodInfo.Invoke(observed, parameters.ToArray()); });
                else
                    Debug.Log("The Method with the given name " + levelObjectAction.ActionName +
                              " could not be found, is it private?");
            }
        }


        // THIS NEEDS A SAFETY LIST LATER

        // NOT COMPLETED
        // THIS NEEDS TO PACK ARGUMENTS INTO ANY
        public virtual void SendAction(string actionName, ActionParam argument)
        {
            var action = new LevelObjectAction
            {
                ActionName = actionName
            };
            var arguments = new Dictionary<int, Parameter>();

            var packedArgument = argument.Pack();

            arguments.Add(packedArgument.Item1, packedArgument.Item2);

            action.Params.Add(arguments);
            Marshall(action,TCP: false);
        }


        public void SendAction(string actionName)
        {
            var action = new LevelObjectAction
            {
                ActionName = actionName
            };
            Marshall(action,TCP: false);
        }


        public class ActionParam
        {
            public object data;
            public int FieldPlace;
            public Type type;

            internal static ActionParam From<T>(T argument, int fieldPlace = 0)
            {
                var actionParam = new ActionParam();
                actionParam.FieldPlace = fieldPlace;
                actionParam.type = argument.GetType();
                actionParam.data = argument;
                return actionParam;
            }


            public static ActionParam Unpack(Tuple<int, Parameter> data)
            {
                var actionParam = new ActionParam();
                actionParam.FieldPlace = data.Item1;

                actionParam.type = Type.GetType(data.Item2.Type);


                Debug.Log("Unpacking " + actionParam.type);

                if (actionParam.type == typeof(ChunkData))
                {
                    var netChunkData = data.Item2.Value.Unpack<NetLevel.ChunkData>();
                    actionParam.data = ChunkData.FromNetData(netChunkData);
                }
                else if (actionParam.type == typeof(bool))
                {
                    actionParam.data = data.Item2.Value.Unpack<BoolValue>().Value;
                }
                else if (actionParam.type.IsSubclassOf(typeof(NetworkLevelObject)) ||
                         actionParam.type == typeof(NetworkLevelObject))
                {
                    var identityOfParam = data.Item2.Value.Unpack<StringValue>().Value;

                    var h = FindByIdentity<LevelObjectNetworkHandler>(identityOfParam);

                    Debug.Log("Matched with Reference: " + identityOfParam + " " + h.observed);
                    actionParam.data = h.observed;
                }


                return actionParam;
            }


            public (int, Parameter) Pack()
            {
                Any x = null;


                Debug.Log("Packing " + type);

                if (type == typeof(ChunkData))
                {
                    var chunkData = ChunkData.ToNetData((ChunkData) data);
                    x = Any.Pack(chunkData);
                }
                else if (type == typeof(bool))
                {
                    var bv = new BoolValue();
                    bv.Value = (bool) data;
                    x = Any.Pack(bv);
                }
                else if (type.IsSubclassOf(typeof(NetworkLevelObject)) || type == typeof(NetworkLevelObject))
                {
                    var h = ((NetworkLevelObject) data).levelObjectNetworkHandler;
                    var sv = new StringValue();
                    sv.Value = h.Identity;
                    Debug.Log("Matched with Reference: " + h.Identity);
                    x = Any.Pack(sv);
                }

                var p = new Parameter
                {
                    Type = type.FullName,
                    Value = x
                };
                return (FieldPlace, p);
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