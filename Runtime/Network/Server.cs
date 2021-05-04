using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using General;
using Google.Protobuf;

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
                Client c = new Client(tpClient,i);
                clients[i] = c;
                await c.Process();
                c.Disconnect();

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
                if(clients[i] != null)
                    clients[i].Disconnect();
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

        public void WriteAll(IMessage message)
        {
            for (int i = 0; i < 4; i++)
            {
                if (clients[i] != null)
                {
                    clients[i].WritePacket(message);
                }
            }
        }

        public void StopListen()
        {
            listener.Stop();
        }
    }
}
