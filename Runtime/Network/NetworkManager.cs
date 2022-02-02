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
            playerSpawned = 2,
            readyLobby = 3,
            removeEntity,
            createEntity,
            entityState,
            streamChunk,
            removePlayer,
            createPlayer,
            syncVar,
            lobbyRequest,
            startRound,
            winRound,
            readyRound
        }

        public enum ClientToServerId : ushort
        {
            processAction = 1,
            name = 2,
            prepareRound = 3,
            playerInput,
            readyLobby
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
                server = new Server();
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
                localId = -1;
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