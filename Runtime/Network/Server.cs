using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using General;
using Google.Protobuf;
using System.Threading;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Server
    {
        public Client[] clients;
        
        int port = 13565;

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        TcpListener listener;

        public static Server instance;


        public Server()
        {
            if(instance == null)
            {
                instance = this;
            }
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




        

        public void Start()
        {
            clients = new Client[4];
            listener = new TcpListener(this.localAddr, this.port);
            listener.Start();
            Thread createThread = new Thread(new ThreadStart(() =>
            {
                Task.Run(HandleNewConnection);
            }));
            createThread.IsBackground = true;
            createThread.Start();

        }

        async Task HandleNewConnection()
        {
            TcpClient tpClient = await listener.AcceptTcpClientAsync();
            int i = GetFreeSlot();
            int udpPort = port + 1 + i;
            Debug.Log("Reading on "+ udpPort);
            UdpClient udClient = new UdpClient(udpPort);
            if (i == -1)
            {
                tpClient.Close();
            }
            else
            {
                Client c = new Client(tpClient,udClient,i,port);
                clients[i] = c;
                Thread handleThread = new Thread(new ThreadStart(() =>
                {
                    Task.Run(async () => { await c.Process(); c.Disconnect(); });
                    
                }));
                handleThread.IsBackground = true;
                handleThread.Start();

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

        public void Disconnect(int localId)
        {

            if (clients[localId] != null)
            { 
                clients[localId].Disconnect();
                clients[localId] = null;
            }
        }


        public  void WriteAll(Packet p)
        {
            for(int i = 0;i<4;i++)
            {
                if(clients[i] != null)
                {
                    clients[i].WritePacket(p);
                }
            }
        }

        public void WriteAll(IMessage message, bool TCP = true)
        {
            for (int i = 0; i < 4; i++)
            {
                if (clients[i] != null)
                {
                    clients[i].WritePacket(message, TCP: TCP);
                }
            }
        }

        public void WriteAll(IMessage message, int except, bool TCP = true)
        {
            for (int i = 0; i < 4 && (i != except); i++)
            {
                if (clients[i] != null)
                {
                    clients[i].WritePacket(message,TCP: TCP);
                }
            }
        }

        public void StopListen()
        {
            listener.Stop();
        }
    }
}
