using System;
using Google.Protobuf;
using NetLevel;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.Networking;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkManagerHandler : NetworkHandler
    {


        public static void RequestLobbyUpdate(LevelMetaData selectedLevel)
        {
            
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ClientToServerId.lobbyUpdate);
            m.AddBytes(selectedLevel.ToByteArray());
            NetworkManager.instance.client.Send(m);
        }


        [MessageHandler((ushort)NetworkManager.ClientToServerId.lobbyUpdate)]
        public static void OnLobbyRequestUpdate(Message m)
        {
            if (NetworkManager.instance.isOnServer)
            {
                NetworkManager.instance.server.SendToAll(m);
            }
            var metaData = LevelMetaData.Parser.ParseFrom(m.GetBytes());
            NetworkManager.lobbyRequestEvent.Invoke(metaData);

        }
        
        public static void RequestReadyRound()
        {
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ServerToClientId.readyRound);

            m.AddBool(NetworkManager.instance.ready);
            m.AddInt(NetworkManager.instance.localId);
            NetworkManager.instance.server.SendToAll(m);
        }

        public static void RequestReadyLobby()
        {
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ClientToServerId.readyLobby);
            m.AddInt(NetworkManager.instance.localId);
            NetworkManager.instance.client.Send(m);
        }

        [MessageHandler((ushort)NetworkManager.ClientToServerId.readyLobby)]
        public static void OnReadyLobby(Message m)
        {
            if (NetworkManager.instance.isOnServer)
            {
                
                    int localId = m.GetInt();
                    Vector3 location = new Vector3(localId * 8, 6, 0);
                    GameConsole.Log($"Teleporting {localId} to {location}");
                    PlayerManager.playerManager.SpawnPlayer(localId, location);
            }
        }


        public static void RequestPrepareRound(LevelMetaData metaData)
        {
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ClientToServerId.readyLobby);
            m.AddBytes(metaData.ToByteArray());
            NetworkManager.instance.server.SendToAll(m);
        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.prepareRound)]
        public static void PrepareRound(Message m)
        {
            GameConsole.Log("Preparing Round");
            MainCaller.Do(() => { NetworkManager.prepareRoundEvent.Invoke(); });
            NetLevel.LevelMetaData netData = LevelMetaData.Parser.ParseFrom(m.GetBytes());
            if (netData != null)
            {
                var levelMetaData = netData;
                MainCaller.Do(() => { LevelDataManager.New(levelMetaData, saveImmediately: false); });
            }
        }


        [MessageHandler((ushort)NetworkManager.ServerToClientId.startRound)]
        public static void StartRound(Message m)
        {
            MainCaller.Do(() =>
            {
                NetworkManager.startRoundEvent.Invoke();
                PlayerManager.acceptInput = true;
                PlayerManager.SetPlayerActive(NetworkManager.instance.localId,true);
            });
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.winRound)]
        public static void WinRound(Message m)
        {
            MainCaller.Do(() =>
            {
                NetworkManager.winEvent.Invoke();
                PlayerManager.acceptInput = false;
            });
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.readyRound)]
        public static void ReadyRound(Message m)
        {
            int localId = m.GetInt();
            bool ready = m.GetBool();
            
                if (NetworkManager.instance.isOnServer)
                {
                    NetworkManager.instance.server.SendToAll(m);
                }
                GameConsole.Log("Received Ready round");
                NetworkManager.readyEvent.Invoke(new Tuple<int, bool>(localId, ready));
        }
    }
}