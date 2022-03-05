using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NetLevel;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        public static List<NetworkHandler> networkHandlers = new List<NetworkHandler>();

        public static bool isConnected;

        public Option useUDPOption;
        
        public static string userName;

        public static UnityEvent connectEvent = new UnityEvent();
        public static UnityEvent disconnectEvent = new UnityEvent();
        public static UnityEvent prepareRoundEvent = new UnityEvent();
        public static UnityEvent startRoundEvent = new UnityEvent();
        public static UnityEvent winEvent = new UnityEvent();
        public static UnityEvent<Tuple<int, bool>> readyEvent = new UnityEvent<Tuple<int, bool>>();

        public static UnityEvent<LevelMetaData> lobbyRequestEvent = new UnityEvent<LevelMetaData>();
        

        public bool isOnServer = false;

        public int localId;
        
        public bool ready;

        public Server Server { get; private set; }
        public Client Client { get; private set; }
        
        // Temporary fix

        public bool useUDP;
        
        public enum ServerToClientId : ushort
        {
            processAction = 1,
            readyRound = 2,
            removeEntity = 3,
            createEntity = 4,
            streamChunk = 5,
            removePlayer = 6,
            createPlayer = 7,
            syncVar = 8,
            startRound = 9,
            winRound = 10,
            prepareRound = 11,
            setProperty = 12,
            entityState = 13
        }

        public enum ClientToServerId : ushort
        {
            readyRound = 12,
            playerInput = 13,
            readyLobby = 14,
            lobbyUpdate  = 15,
        }

        private void Awake()
        {
            if (instance != null)
                Destroy(this);
            instance = this;
        }
        
        // Start is called before the first frame update
        private void Start()
        {


            // THIS IS A STUPID PLACE BUT WILL CHANGE LATER
            Time.fixedDeltaTime = 0.02f;

            
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
            
            if (isOnServer)
            {
                localId = -1;
                Server = new Server();
                Server.Start(13565,4);
                Server.ClientConnected += OnClientConnect;
                Server.ClientDisconnected += OnClientDisconnect;
            }
            else
            {
                
                Client = new Client();
                Client.Connected += OnConnected;
                Client.Connected += (a,b) => { NetworkManager.networkHandlers = new List<NetworkHandler>();
                    localId = (ushort) Client.Id;
                };
                Client.ClientDisconnected += OnClientDisconnect;
                Client.Disconnected += OnDisconnect;
                Client.ConnectionFailed += FailedToConnect;
            }


        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            GameConsole.Log("Could not connect");
        }
        
        private int count = 8;
       
        public void FixedUpdate()
        {
            
                LevelObjectBaseAnimator.NetTick();
            
            
                for (int i = 0; i < count;i++)
                {
                    if (LevelObjectNetworkHandler.propertyRequests.Count > 0)
                    {
                        LevelObjectNetworkHandler.ProcessPropertyRequest(LevelObjectNetworkHandler.propertyRequests.Dequeue());
                    }
                }
                if (isOnServer)
                {
                    Server.Tick();
                }
                else
                {
                    Client.Tick();
                }
        }


        public void ResetHandlers()
        {
            foreach (var handler in networkHandlers)
            {
                if(handler != null)
                    handler.ResetHandler();
            }
        }
        
        
        //Factor this out into GameLogic

        

        public void OnClientConnect(object sender, ServerClientConnectedEventArgs e)
        {
            int newLocalId = e.Client.Id - 1;
            PlayerManager.playerManager.Add(newLocalId,"Test",true,null);
            for(int i = 0; i < 4;i++)
            {
                if(i != newLocalId)
                {
                    Player p = PlayerManager.playerManager.players[i];
                    if(p != null)
                    {
                        PlayerNetworkHandler playerNetworkHandler = (PlayerNetworkHandler) p.levelObjectNetworkHandler;
                        playerNetworkHandler.RequestCreation();
                    }
                }
            }
        }
        
        public void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e)
        {
            int newLocalId = e.Id - 1;
            PlayerManager.playerManager.Remove(newLocalId);
        }
        
        public void OnDisconnect(object sender, EventArgs e)
        {
            disconnectEvent.Invoke();
            PlayerManager.playerManager.RemoveAll();
        }

        public void OnDestroy()
        {
            if (isOnServer)
            {
                DisconnectAllClients();
            }
            else
            {
                Disconnect();
            }
        }

        public void DisconnectAllClients()
        {
            for (int i = 1; i <= 4; i++)
            {
                Server.DisconnectClient((ushort) i);
            }
        }

        public void Connect(string ip, string playerName, Action onConnect)
        {
            if (!Client.IsConnected)
            {
                userName = playerName;
                Client.Connect($"{ip}:13565");
                GameConsole.Log($"Set new client {Client}");
            }
        }

        public void OnConnected(object sender, EventArgs e)
        {
                localId = Client.Id - 1;
                isConnected = true;
                SetNetworkHandlers(isConnected);
        }

        private void SetNetworkHandlers(bool v)
        {
            foreach (var h in networkHandlers) 
            {
                if(h!=null)
                    h.enabled = v;
            }
        }

        public void Disconnect(bool respond = true)
        {
            Client.Disconnect();
        }

        public void CallReady(bool r)
        {
            ready = r;
            NetworkManagerHandler.RequestReadyRound();
        }

        public void CallLobbyReady()
        {
            NetworkManagerHandler.RequestReadyLobby();
        }

        public void CallSelected(LevelMetaData metaData)
        {
            NetworkManagerHandler.RequestLobbyUpdate(metaData);
        }

        

        public void OnApplicationQuit()
        {
            Disconnect();
        }

        
    }
}