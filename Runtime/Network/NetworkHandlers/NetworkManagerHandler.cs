using System;
using General;
using NetLevel;
using RiptideNetworking;
using State;
using UnityEngine;
using UnityEngine.Networking;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkManagerHandler : NetworkHandler
    {


        public NetworkManager GetObserved()
        {
            return (NetworkManager) observed;
        }
        
        
        public static void RequestPrepareRound()
        {
            var prepareRound = new PrepareRound
            {
                Message = "READY, STEADY",
                LevelMetaData = LevelManager.currentLevelMetaData
            };
            Marshall(typeof(NetworkManagerHandler), prepareRound);
        }

        public static void RequestStartRound()
        {
            var startRound = new StartRound
            {
            };
            Marshall(typeof(NetworkManagerHandler), startRound,TCP: true);
        }

        public static void RequestWinRound()
        {
            var winRound = new WinRound();
            Marshall(typeof(NetworkManagerHandler), winRound);
        }

        public static void RequestLobbyUpdate(LevelMetaData selectedLevel)
        {
            LobbyRequest updateRequest = new LobbyRequest();
            updateRequest.SelectedLevel = selectedLevel;
            Marshall(typeof(NetworkManagerHandler), updateRequest);
        }
        
        public static void RequestReadyRound()
        {
            var readyRound = new ReadyRound();
            readyRound.Ready = NetworkManager.instance.ready;
            readyRound.LocalId = NetworkManager.instance.localId;
            Marshall(typeof(NetworkManagerHandler), readyRound);
        }

        public static void RequestReadyLobby()
        {
            ReadyLobby readyLobby = new ReadyLobby();
            readyLobby.Message = "Test";
            Marshall(typeof(NetworkManagerHandler), readyLobby);
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.readyLobby)]
        public static void OnReadyLobby(Packet p)
        {
            if (NetworkManager.instance.isOnServer)
            {
                ReadyLobby readyLobby;
                if (p.Content.TryUnpack(out readyLobby))
                {
                    int localId = p.Sender;
                    Vector3 location = new Vector3(localId * 8, 6, 0);
                    GameConsole.Log($"Teleporting {localId} to {location}");
                    PlayerManager.playerManager.SpawnPlayer(localId, location);
                }
            }
        }
        

        [MessageHandler((ushort)NetworkManager.ClientToServerId.prepareRound)]
        public static void PrepareRound(Packet p)
        {
            GameConsole.Log("Preparing Round");
            MainCaller.Do(() => { NetworkManager.prepareRoundEvent.Invoke(); });
            NetLevel.LevelMetaData netData = null;
            PrepareRound prepareRound;
            if (p.Content.TryUnpack(out prepareRound)) netData = prepareRound.LevelMetaData;
            if (netData != null)
            {
                var levelMetaData = netData;
                MainCaller.Do(() => { LevelDataManager.New(levelMetaData, saveImmediately: false); });
            }
        }


        [MessageHandler((ushort)NetworkManager.ServerToClientId.lobbyRequest)]
        public static void OnLobbyRequest(Packet p)
        {
            LobbyRequest lobbyRequest;
            if (p.Content.TryUnpack<LobbyRequest>(out lobbyRequest))
            {
                if (NetworkManager.instance.isOnServer)
                {
                    Marshall(typeof(NetworkManagerHandler),lobbyRequest);
                }
                NetworkManager.lobbyRequestEvent.Invoke(lobbyRequest);
            }
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.startRound)]
        public static void StartRound(Packet p)
        {
            MainCaller.Do(() =>
            {
                NetworkManager.startRoundEvent.Invoke();
                PlayerManager.acceptInput = true;
                PlayerManager.SetPlayerActive(NetworkManager.instance.localId,true);
            });
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.winRound)]
        public static void WinRound(Packet p)
        {
            MainCaller.Do(() =>
            {
                NetworkManager.winEvent.Invoke();
                PlayerManager.acceptInput = false;
            });
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.readyRound)]
        public static void ReadyRound(Packet p)
        {
            ReadyRound readyRound;
            if (p.Content.TryUnpack(out readyRound))
            {
                if (isOnServer)
                {
                    Marshall(typeof(NetworkManagerHandler),readyRound);
                }
                GameConsole.Log("Received Ready round");
                NetworkManager.readyEvent.Invoke(new Tuple<int, bool>(readyRound.LocalId, readyRound.Ready));
                
            }
        }
    }
}