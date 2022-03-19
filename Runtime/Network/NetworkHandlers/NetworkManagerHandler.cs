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
        public static void RequestStartRound()
        {
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ServerToClientId.startRound);
            NetworkManager.instance.Server.SendToAll(m);
        }

        public static void RequestWinRound()
        {
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ServerToClientId.winRound);
            NetworkManager.instance.Server.SendToAll(m);
        }

        public static void UpdatePlayerConfig(string playerName)
        {
            Message m = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ClientToServerId.playerConfig);
            m.AddString(playerName);
            NetworkManager.instance.Client.Send(m); 
        }

        [MessageHandler((ushort) NetworkManager.ClientToServerId.playerConfig)]
        public static void OnPlayerConfigUpdate(ushort id, Message m)
        {
            int localId = id - 1;
            string name = m.GetString();
            PlayerManager.playerManager.players[localId].playerName = name;
        }
        

        public static void RequestLobbyUpdate(LevelMetaData selectedLevel)
        {

            Message m = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ClientToServerId.lobbyUpdate);
            m.AddLong(selectedLevel.UniqueLevelId);
            NetworkManager.instance.Client.Send(m);
        }


        [MessageHandler((ushort)NetworkManager.ClientToServerId.lobbyUpdate)] 
        public static void OnLobbyRequestUpdate(ushort id, Message m)
        {

            int localId = id - 1;
            LevelMetaData metaData = new LevelMetaData
            {
                UniqueLevelId = m.GetLong()
            };

            Message answer = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.lobbyUpdate);
            answer.AddLong(metaData.UniqueLevelId);
            GameConsole.Log($"{localId} selected Level: {metaData}");
            if (NetworkManager.instance.isOnServer)
            {
                NetworkManager.instance.Server.SendToAll(answer);
            }
            NetworkManager.lobbyRequestEvent.Invoke(metaData);

        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.lobbyUpdate)] 
        public static void OnLobbyRequestUpdate(Message m)
        {

            LevelMetaData metaData = new LevelMetaData
            {
                UniqueLevelId = m.GetLong()
            };
            NetworkManager.lobbyRequestEvent.Invoke(metaData);

        }
        
        public static void RequestReadyRound()
        {
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ClientToServerId.readyRound);
            m.AddBool(NetworkManager.instance.ready);
            NetworkManager.instance.Client.Send(m);
        }

        public static void RequestReadyLobby()
        {
            Message m = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ClientToServerId.readyLobby);
            NetworkManager.instance.Client.Send(m);
        }

        [MessageHandler((ushort)NetworkManager.ClientToServerId.readyLobby)]
        public static void OnReadyLobby(ushort id,Message m)
        {
            if (NetworkManager.instance.isOnServer)
            {
                int localId = id - 1; 
                Vector3 location = new Vector3(localId * 8, 6, 0);
                GameConsole.Log($"Teleporting {localId} to {location}");
                PlayerManager.playerManager.SpawnPlayer(localId, location);
            }
        }
        
        /*

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
        */

        public static void RequestPrepareRound(LevelMetaData metaData)
        {
            GameConsole.Log("Calling for Preparation");
            Message m = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ServerToClientId.prepareRound);
            m.AddLong(metaData.UniqueLevelId);
            NetworkManager.instance.Server.SendToAll(m);
        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.prepareRound)]
        public static void PrepareRound(Message m)
        {
            GameConsole.Log("Preparing Round");
            long uniqueLevelId = m.GetLong(); 
            NetworkManager.prepareRoundEvent.Invoke();
            NetLevel.LevelMetaData netData = new LevelMetaData();
            netData.UniqueLevelId = uniqueLevelId;
            if (netData != null)
            {
                var levelMetaData = netData;
                LevelDataManager.New(levelMetaData, saveImmediately: false);
            }
            
        }


        [MessageHandler((ushort)NetworkManager.ServerToClientId.startRound)]
        public static void StartRound(Message m)
        {
                GameConsole.Log($"Received Start Round for LocalId {NetworkManager.instance.localId}");
                PlayerManager.SetPlayerActive(NetworkManager.instance.localId,true);
                PlayerManager.playerManager.SetCurrentPlayer(NetworkManager.instance.localId);
                NetworkManager.startRoundEvent.Invoke();
                PlayerManager.acceptInput = true;
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.winRound)]
        public static void WinRound(Message m)
        {
                NetworkManager.winEvent.Invoke();
                PlayerManager.acceptInput = false;
        }

        [MessageHandler((ushort)NetworkManager.ClientToServerId.readyRound)]
        public static void ReadyRound(ushort id,Message m)
        {
            int localId = id - 1;
            bool ready = m.GetBool();
            GameConsole.Log($"Received Ready round {localId} {ready}");
            
                if (NetworkManager.instance.isOnServer)
                {
                    Message toClient = Message.Create(MessageSendMode.reliable,(ushort) NetworkManager.ServerToClientId.readyRound);
                    toClient.AddInt(localId);
                    toClient.AddBool(ready);
                    NetworkManager.instance.Server.SendToAll(toClient);
                }
                NetworkManager.readyEvent.Invoke(new Tuple<int, bool>(localId, ready));
        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.readyRound)]
        public static void ReadyRound(Message m)
        {
            int localId = m.GetInt();
            bool ready = m.GetBool();
            GameConsole.Log($"Received Ready round {localId} {ready}");
            NetworkManager.readyEvent.Invoke(new Tuple<int, bool>(localId, ready));
            GameConsole.Log("Ready call called");
        }
    }
}