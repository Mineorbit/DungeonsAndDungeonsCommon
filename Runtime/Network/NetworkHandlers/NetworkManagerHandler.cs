using System;
using General;
using State;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkManagerHandler : NetworkHandler
    {
        public new NetworkManager observed;

        public static void RequestPrepareRound()
        {
            var prepareRound = new PrepareRound
            {
                Message = "READY, STEADY",
                LevelMetaData = LevelMetaData.ToNetData(LevelManager.currentLevelMetaData)
            };
            Marshall(typeof(NetworkManagerHandler), prepareRound);
        }

        public static void RequestStartRound()
        {
            var startRound = new StartRound
            {
                Message = "GO"
            };
            Marshall(typeof(NetworkManagerHandler), startRound);
        }

        public static void RequestWinRound()
        {
            var winRound = new WinRound();
            Marshall(typeof(NetworkManagerHandler), winRound);
        }

        public static void RequestReadyRound()
        {
            var readyRound = new ReadyRound();
            readyRound.Ready = NetworkManager.instance.ready;
            readyRound.LocalId = NetworkManager.instance.localId;
            Marshall(typeof(NetworkManagerHandler), readyRound);
        }

        [PacketBinding.Binding]
        public static void OnDisconnect(Packet p)
        {
            MeDisconnect meDisconnect;
            if (p.Content.TryUnpack(out meDisconnect))
            {
                var localId = p.Sender;

                if (isOnServer)
                {
                    Server.instance.clients[localId].Disconnect(false);
                    PlayerManager.playerManager.Remove(localId);
                }
                else
                {
                    NetworkManager.instance.Disconnect(false);
                }
            }
        }


        [PacketBinding.Binding]
        public static void PrepareRound(Packet p)
        {
            Debug.Log("Preparing Round");
            MainCaller.Do(() => { NetworkManager.prepareRoundEvent.Invoke(); });
            NetLevel.LevelMetaData netData = null;
            PrepareRound prepareRound;
            if (p.Content.TryUnpack(out prepareRound)) netData = prepareRound.LevelMetaData;
            if (netData != null)
            {
                var levelMetaData = LevelMetaData.FromNetData(netData);
                MainCaller.Do(() => { LevelDataManager.New(levelMetaData, saveImmediately: false); });
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

        [PacketBinding.Binding]
        public static void ReadyRound(Packet p)
        {
            ReadyRound readyRound;
            if (p.Content.TryUnpack(out readyRound))
            {
                if (isOnServer)
                {
                    Marshall(typeof(NetworkManagerHandler),readyRound);
                }
                NetworkManager.readyEvent.Invoke(new Tuple<int, bool>(readyRound.LocalId, readyRound.Ready));
                
            }
        }
    }
}