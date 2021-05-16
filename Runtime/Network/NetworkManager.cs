using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using UnityEngine.Events;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        public List<PacketBinding> packetBindings = new List<PacketBinding>();

        public static List<NetworkHandler> networkHandlers = new List<NetworkHandler>();

        public static bool isConnected;

        
        public Client client;

        public static List<Client> allClients =  new List<Client>();

        public int localId;
        public static string userName;

        public static UnityEvent connectEvent = new UnityEvent();
        public static UnityEvent disconnectEvent = new UnityEvent();
        public static UnityEvent prepareRoundEvent = new UnityEvent();
        public static UnityEvent   startRoundEvent = new UnityEvent();
        public static UnityEvent winEvent = new UnityEvent();

        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
                Destroy(this);
            instance = this;


            // THIS IS A STUPID PLACE BUT WILL CHANGE LATER
            Time.fixedDeltaTime = 0.01f;

            foreach (PacketBinding p in packetBindings)
            {
                p.AddToBinding();
            }
        }

        static Action onConnectAction;
        public void Connect(string ip, string playerName, Action onConnect)
        {
            if(!isConnected)
            {
            onConnectAction = onConnect;
            userName = playerName;
            Task<Client> t = Task.Run(async () => await Client.Connect(System.Net.IPAddress.Parse(ip), 13565));
            client = t.Result;
            client.onConnectEvent.AddListener(OnConnected);
                

                //Task<Client> t = Task.Run(Client.Connect(System.Net.IPAddress.Parse("127.0.0.1"), 13565));

            }

        }

        public  void OnConnected(int id)
        {
            MainCaller.Do(() =>
            {
                localId = id;
                isConnected = true;
                SetNetworkHandlers(isConnected);
                onConnectAction.Invoke();
            });
        }

        void SetNetworkHandlers(bool v)
        {
            foreach(NetworkHandler h in networkHandlers)
            {
                h.enabled = v;
            }
        }

        

        public void FixedUpdate()
        {
            foreach(Client c in allClients)
            {
                c.FixedUpdate();
            }
        }

        public void Disconnect(bool respond = true)
        {
            if(isConnected)
            { 
                isConnected = false;
                client.Disconnect(respond: respond);
                disconnectEvent.Invoke();
            }
        }

        //Factor this out into GameLogic

        

        public void OnDestroy()
        {
            Disconnect();
        }
    }
}
