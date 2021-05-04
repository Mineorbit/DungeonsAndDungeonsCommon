using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using State;
using System.IO;
using General;
using System.Threading;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Client
    {
        public TcpClient tcpClient;
        public UdpClient udpClient;


        public bool isOnServer;

        public bool Connected = false;

        public string userName;
        public int localid;

        Semaphore waitingForSpecific = new Semaphore(1,1);

        public NetworkStream tcpStream;


        public override string ToString()
        {
            return "NetworkClient "+userName+" "+localid;
        }

        public Client(TcpClient tcpC, int lId)
        {
            Connected = true;
            tcpClient = tcpC;
            tcpStream = tcpClient.GetStream();
            localid = lId;
        }

        public Client(TcpClient tcpC)
        {
            Connected = true;
            tcpClient = tcpC;
            tcpStream = tcpClient.GetStream();
            localid = -1;
        }
        public Client()
        {

        }

        ~Client()
        {
            Disconnect();
        }

        

        public static async Task<Client> Connect(IPAddress host, int port)
        {

                Client client = new Client();

                Thread createThread = new Thread(new ThreadStart(() => { CreateTcpClientForClient(client, host, port); client.Connected = true; }));
                createThread.IsBackground = true;
                createThread.Start();

                

                return client;
        }

        public void Disconnect(bool respond = true)
        {
            if(Connected)
            { 
            if(respond)
            {
                MeDisconnect meDisconnect = new MeDisconnect();
                WritePacket(meDisconnect);
            }
                MainCaller.Do(() => { PlayerManager.playerManager.Remove(localid); });

                tcpStream.Close();
            tcpClient.Close();
            Connected = false;
            }
        }

        //evtl async später
        public static void CreateTcpClientForClient(Client client,IPAddress host, int port)
        {
            client.tcpClient = new TcpClient(host.ToString(), port);
            client.tcpClient.SendTimeout = 1000;
            client.tcpStream = client.tcpClient.GetStream();
            client.Setup();
        }

        public static void CreateUdpClientForClient(Client client, IPAddress host, int port)
        {
            client.tcpClient = new TcpClient(host.ToString(), port);
            client.tcpClient.SendTimeout = 1000;
            client.Setup();
        }

        public void WritePacket(IMessage message)
        {

            General.Packet p = new General.Packet
            {
                Type = message.GetType().ToString(),
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message)
            };

            WritePacket(p);

        }

        public void WritePacket(Packet p)
        {

            byte[] data = p.ToByteArray();
            int length = data.Length;
            byte[] result = new byte[length + 4];
            byte[] lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
                Array.Reverse(data);
            }
            Array.Copy(lengthBytes, 0, result, 0, 4);
            Array.Copy(data, 0, result, 4, length);

            tcpStream.Write(result, 0, result.Length);
            Debug.Log("Sent " + p);

        }

        async Task<byte[]> ReadData()
        {
            waitingForSpecific.WaitOne();
            byte[] lengthBytes = new byte[4];
            await tcpStream.ReadAsync(lengthBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] data = new byte[length];
            await tcpStream.ReadAsync(data, 0, length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            waitingForSpecific.Release();
            return data;
        }

        // int tries; necessary?
        public async Task<T> ReadPacket<T>() where T : IMessage, new()
        {
            Debug.Log("Waiting for Data");
            byte[] data = await ReadData();

            var p = General.Packet.Parser.ParseFrom(data);
            Debug.Log("Received "+p);
            T result;
            if (p.Content.TryUnpack<T>(out result))
            {
                return result;
            }else
            {
                //handle packet otherwise?
                return await ReadPacket<T>();
            }
        }



        public void Setup()
        {
            Welcome w = Task.Run(ReadPacket<Welcome>).Result;

            localid = w.LocalId;

            MeConnect meConnect = new MeConnect
            {
                Name = NetworkManager.userName
            };

            WritePacket(meConnect);

            Task.Run(HandlePackets);
        }

        // This needs to be exited after some kind of timeout
        public async Task Process()
        {

            Welcome w = new Welcome
            {
                LocalId = localid
            };

            //Send welcome

            WritePacket(w);

            Debug.Log("Sent "+w);

            MeConnect meConnect = await ReadPacket<MeConnect>();
            userName = meConnect.Name;

            Connected = true;

            MainCaller.Do(()=> { PlayerManager.playerManager.Add(localid, userName, true); });
            
            await HandlePackets();
        }

        public async Task HandlePackets()
        {
            byte[] data = await ReadData();

            if(!Connected)
            {
                return;
            }

            Packet p = General.Packet.Parser.ParseFrom(data);

            Type packetType = Type.GetType(p.Type);

            if(packetType == typeof(MeDisconnect))
            {
                Disconnect(respond: false);
            }else
            {
                NetworkHandler.UnMarshall(p);
            }

            //Processing needed

            await HandlePackets();
        }
    }
}
