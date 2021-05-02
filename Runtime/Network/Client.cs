using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Client
    {
        public TcpClient tcpClient;
        public UdpClient udpClient;

        int localid;

        public NetworkStream tcpStream;

        byte[] buffer = new byte[4];
        public Client(TcpClient tcpC, int lId)
        {
            tcpClient = tcpC;
            localid = lId;
        }

        public Client(TcpClient tcpC)
        {
            tcpClient = tcpC;
            localid = -1;
        }

        public static async Task<Client> Connect(IPAddress host, int port)
        {
            TcpClient tClient = new TcpClient();
            await tClient.ConnectAsync(host, port);

            Client client = new Client(tClient);
            return client;
        }
        // This needs to be exited when no more messages  are  received
        public async Task Process()
        {
            tcpStream = tcpClient.GetStream();
            

            //Send welcome


        }
    }
}
