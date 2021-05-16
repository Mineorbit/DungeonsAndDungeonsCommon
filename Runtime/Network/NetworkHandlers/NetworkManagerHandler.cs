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
        public new NetworkManager observed;
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

        public static void RequestWinRound()
        {
            State.WinRound winRound = new State.WinRound();
            Marshall(typeof(NetworkManagerHandler), winRound);
        }

        [PacketBinding.Binding]
        public static void OnDisconnect(Packet p)
        {
            MeDisconnect meDisconnect;
            if(p.Content.TryUnpack<MeDisconnect>(out meDisconnect))
            {
                int localId = p.Sender;

                if(isOnServer)
                {
                    Server.instance.clients[localId].Disconnect(respond: false);
                    PlayerManager.playerManager.Remove(localId);
                }
                else {
                    NetworkManager.instance.Disconnect(respond: false);
                }
            }
        }


        [PacketBinding.Binding]
        public static void PrepareRound(Packet p)
        {
            Debug.Log("Preparing Round");
            MainCaller.Do(() =>
            {
                NetworkManager.prepareRoundEvent.Invoke();
            });
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
                NetworkManager.startRoundEvent.Invoke();
                PlayerManager.acceptInput = true;
            });
        }

        [PacketBinding.Binding]
        public static void WinRound(Packet p)
        {
            MainCaller.Do(() => { NetworkManager.winEvent.Invoke(); });
        }
    }
}