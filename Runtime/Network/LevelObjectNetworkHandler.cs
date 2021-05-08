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
