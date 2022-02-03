using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        
        private static Action onConnectAction;

        public bool isOnServer = false;

        public int localId;
        
        public bool ready;

        public Server server;
        public Client client;

        
        // Temporary fix

        public bool useUDP;
        
        public enum ServerToClientId : ushort
        {
            processAction = 1,
            removeEntity = 2,
            createEntity = 3,
            streamChunk = 4,
            removePlayer = 5,
            createPlayer = 6,
            syncVar = 7,
            startRound = 8,
            winRound = 9,
            readyRound = 10,
            prepareRound = 11
        }

        public enum ClientToServerId : ushort
        {
            processAction = 1,
            playerInput = 2,
            readyLobby = 3,
            lobbyUpdate  = 4
        }
        
        
        // Start is called before the first frame update
        private void Start()
        {
            if (instance != null)
                Destroy(this);
            instance = this;


            // THIS IS A STUPID PLACE BUT WILL CHANGE LATER
            Time.fixedDeltaTime = 0.03f;

            
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
            
            if (isOnServer)
            {
                localId = -1;
                server = new Server();
                server.Start(13565,4);
            }
            else
            {
                
                client = new Client();
                client.Connected += OnConnected;
                client.Connected += (a,b) => { NetworkManager.networkHandlers = new List<NetworkHandler>();
                    localId = (ushort) client.Id;
                };

            }


        }

        
        

        public void FixedUpdate()
        {
            if (isOnServer)
            {
                server.Tick();
            }
            else
            {
                client.Tick();
            }
        }


        //Factor this out into GameLogic


        public void OnDestroy()
        {
            Disconnect();
        }

        public void Connect(string ip, string playerName, Action onConnect)
        {
            if (!client.IsConnected)
            {
                userName = playerName;
                client.Connect($"{ip}:13565");
                GameConsole.Log($"Set new client {client}");
            }
        }

        public void OnConnected(object sender, EventArgs e)
        {
            MainCaller.Do(() =>
            {
                localId = client.Id;
                isConnected = true;
                SetNetworkHandlers(isConnected);
                onConnectAction.Invoke();
            });
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
            client.Disconnect();
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