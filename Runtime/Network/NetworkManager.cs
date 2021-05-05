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


        public int localId;
        public static string userName;

        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
                Destroy(this);
            instance = this;
        }

        public void Connect(string playerName, Action onConnect)
        {
            if(!isConnected)
            { 
            userName = playerName;
            Task<Client> t = Task.Run(async () => await Client.Connect(System.Net.IPAddress.Parse("127.0.0.1"), 13565));
            //Task<Client> t = Task.Run(Client.Connect(System.Net.IPAddress.Parse("127.0.0.1"), 13565));
            client = t.Result;

            localId = client.localid;

            isConnected = true;
            onConnect.Invoke();
            }

        }

        public void Update()
        {
            if(!isConnected)
            {

            }
        }

        public void Disconnect()
        {
            if(isConnected)
            { 
            isConnected = false;
            client.Disconnect();
            }
        }
        public void OnDestroy()
        {
            Disconnect();
        }
    }
}
