using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Server
    {
        public Client[] clients;
        
        int port = 13565;

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        TcpListener listener;



        public Server()
        {

        }

        public override string ToString()
        {
            string s = "Server:";
            for(int i = 0;i<4;i++)
            {
                s += "\n"+ i +": "+ clients[i];
            }
            return s;
        }

        public void Disconnect(int localId)
        {
            if(clients[localId] != null)
            { 
            clients[localId].networkStream.Close();
            clients[localId].tcpClient.Close();
            clients[localId] = null;
            }
        }

        public void Start()
        {
            clients = new Client[4];
            listener = new TcpListener(this.localAddr, this.port);
            listener.Start();

            Task.Run(HandleNewConnection);

        }

        async Task HandleNewConnection()
        {
            TcpClient tpClient = await listener.AcceptTcpClientAsync();
            int i = GetFreeSlot();
            
            if(i == -1)
            {
                tpClient.Close();
            }
            else
            {
                Client c = new Client(tpClient);
                clients[i] = c;
                await c.Process();
                Debug.Log("Disconnect");
                Disconnect(i);

            }

            await HandleNewConnection();
        }

        int GetFreeSlot()
        {
            for (int i = 0; i < 4; i++)
                if (clients[i] == null) return i;
            return -1;
        }


        public void Stop()
        {
            DisconnectAll();
            StopListen();
        }

        public void DisconnectAll()
        {
            for(int i = 0; i < 4; i++)
            {
                Disconnect(i);
            }
        }

        public void StopListen()
        {
            listener.Stop();
        }
    }
}
