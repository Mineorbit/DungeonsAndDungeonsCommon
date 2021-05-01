using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        UnityEvent<int> onConnectEvent = new UnityEvent<int>();
        public Client client;
        public int localId;
        // Start is called before the first frame update
        void Start()
        {
            if (instance != null)
                Destroy(this);
            instance = this;
        }

        public void Connect(string playerName)
        {
            Task<Client> t = Task.Run(async () => await Client.Connect(System.Net.IPAddress.Parse("127.0.0.1"), 13565));
            client = t.Result;

            int id = -1;

            onConnectEvent.Invoke(id);
            
        }
    }
}
