using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{ 
public class NetworkManagerHandler : NetworkHandler
{

        public static void RequestPrepareRound()
        {

        }

        public static void RequestStartRound()
        {

        }

        [PacketBinding.Binding]
        public static void PrepareRound()
        {
            LevelDataManager.New(saveImmediately: false);
        }


        [PacketBinding.Binding]
        public static void StartRound()
        {
            PlayerManager.acceptInput = true;
        }
    }
}