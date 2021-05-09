using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General;
using State;
using NetLevel;

namespace com.mineorbit.dungeonsanddungeonscommon
{ 
public class NetworkManagerHandler : NetworkHandler
{

        public static void RequestPrepareRound()
        {
            PrepareRound prepareRound = new PrepareRound
            {
                Message = "READY, STEADY",
                LevelMetaData = LevelMetaData.ToNetData(LevelManager.currentLevelMetaData)
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
            Debug.Log("Preparing Round");
            NetLevel.LevelMetaData netData = null;
            PrepareRound prepareRound;
            if(p.Content.TryUnpack<PrepareRound>(out prepareRound))
            {
                netData = prepareRound.LevelMetaData;
            }
            if(netData != null)
            {
            LevelMetaData levelMetaData = LevelMetaData.FromNetData(netData);
            MainCaller.Do(() => { LevelDataManager.New(levelMetaData,saveImmediately: false); });
            }
        }


        [PacketBinding.Binding]
        public static void StartRound(Packet p)
        {
            MainCaller.Do(() =>
            {
                PlayerManager.acceptInput = true;
            });
        }
    }
}