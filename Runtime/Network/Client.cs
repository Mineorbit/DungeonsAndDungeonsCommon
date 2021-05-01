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

        public NetworkStream networkStream;

        byte[] buffer = new byte[4];
        public Client(TcpClient tcpC)
        {
            tcpClient = tcpC;
        }

        void Connect(IPAddress host, int port)
        {
            tcpClient = new TcpClient();
            tcpClient.ConnectAsync(host, port);
        }
        // This needs to be exited when no more messages  are  received
        public async Task Process()
        {
            networkStream = tcpClient.GetStream();
            int i = 0;
            while(networkStream.Read(buffer,0,1) != 0 && i<4)
            {
                
            }
        }
    }
}
