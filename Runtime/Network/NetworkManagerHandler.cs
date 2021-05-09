using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General;
using State;

namespace com.mineorbit.dungeonsanddungeonscommon
{ 
public class NetworkManagerHandler : NetworkHandler
{

        public static void RequestPrepareRound()
        {
            PrepareRound prepareRound = new PrepareRound
            {
                Message = "READY, STEADY"
            };
            Marshall(typeof(NetworkManagerHandler), prepareRound);
        }

        public static void RequestStartRound()
        {
            StartRound startRound = new StartRound
            {
                Message = "GO"
            };
            Marshall(typeof(NetworkManagerHandler),startRound);
        }

        [PacketBinding.Binding]
        public static void PrepareRound(Packet p)
        {
            Debug.Log("Saas");
            LevelDataManager.New(saveImmediately: false);
        }


        [PacketBinding.Binding]
        public static void StartRound(Packet p)
        {
            PlayerManager.acceptInput = true;
        }
    }
}