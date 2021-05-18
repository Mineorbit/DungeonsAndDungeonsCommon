using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        public static List<NetworkHandler> networkHandlers = new List<NetworkHandler>();

        public static bool isConnected;


        public static List<Client> allClients = new List<Client>();
        public static string userName;

        public static UnityEvent connectEvent = new UnityEvent();
        public static UnityEvent disconnectEvent = new UnityEvent();
        public static UnityEvent prepareRoundEvent = new UnityEvent();
        public static UnityEvent startRoundEvent = new UnityEvent();
        public static UnityEvent winEvent = new UnityEvent();
        public static UnityEvent<Tuple<int, bool>> readyEvent = new UnityEvent<Tuple<int, bool>>();

        private static Action onConnectAction;

        public List<PacketBinding> packetBindings = new List<PacketBinding>();

        public int localId;
        
        public bool ready;


        public Client client;

        // Start is called before the first frame update
        private void Start()
        {
            if (instance != null)
                Destroy(this);
            instance = this;


            // THIS IS A STUPID PLACE BUT WILL CHANGE LATER
            Time.fixedDeltaTime = 0.01f;

            foreach (var p in packetBindings) p.AddToBinding();
            
            
            
        }


        public void FixedUpdate()
        {
            foreach (var c in allClients) c.FixedUpdate();
        }

        //Factor this out into GameLogic


        public void OnDestroy()
        {
            Disconnect();
        }

        public void Connect(string ip, string playerName, Action onConnect)
        {
            if (!isConnected)
            {
                onConnectAction = onConnect;
                userName = playerName;
                var t = Task.Run(async () => await Client.Connect(IPAddress.Parse(ip), 13565));
                
                client = t.Result;
                Debug.Log("Set new client "+client);
                client.onConnectEvent.AddListener(OnConnected);


                //Task<Client> t = Task.Run(Client.Connect(System.Net.IPAddress.Parse("127.0.0.1"), 13565));
            }
        }

        public void OnConnected(int id)
        {
            MainCaller.Do(() =>
            {
                localId = id;
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
            if (!isConnected)
            {
                return;
            }

            isConnected = false;
                if (client != null)
                    client.Disconnect(respond);
                disconnectEvent.Invoke();
        }

        public void CallReady(bool r)
        {
            ready = r;
            NetworkManagerHandler.RequestReadyRound();
        }
    }
}