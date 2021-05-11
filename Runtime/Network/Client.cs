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
using Game;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Client
    {
        public TcpClient tcpClient;
        public UdpClient udpClient;

        public IPEndPoint adressOther;

        public UnityEvent<int> onConnectEvent = new UnityEvent<int>();
        public UnityEvent onDisconnectEvent = new UnityEvent();


        public bool isOnServer;

        public bool Connected = false;

        public string userName;
        public int localid;

        Semaphore waitingForSpecific = new Semaphore(1,1);

        public NetworkStream tcpStream;

        private TaskCompletionSource<bool> disconnected = new TaskCompletionSource<bool>();
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

                Thread createThread = new Thread(new ThreadStart(() => {
                    CreateTcpClientForClient(client, host, port);
                    CreateUdpClientForClient(client,port+1);
                    client.Connected = true; }));

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
            udpClient.Close();
            Debug.Log("We disconnected");
            onDisconnectEvent.Invoke();
            Connected = false;
            disconnected.SetResult(true);
            }
        }

        //evtl async sp�ter
        public static void CreateTcpClientForClient(Client client,IPAddress host, int port)
        {
            client.tcpClient = new TcpClient(host.ToString(), port);
            client.tcpClient.SendTimeout = 1000;
            client.tcpStream = client.tcpClient.GetStream();
            client.Setup();
        }

        public static void CreateUdpClientForClient(Client client, int port)
        {
            client.udpClient = new UdpClient(port);
            client.Setup();
        }

        public void WritePacket(IMessage message,bool TCP = true)
        {

            General.Packet p = new General.Packet
            {
                Type = message.GetType().ToString(),
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message)
            };

            WritePacket(p,TCP: TCP);

        }

        public void WritePacket(Packet p, bool TCP = true)
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

            Debug.Log("Sent: "+p);
            if(TCP && tcpClient.Connected)
            {
                tcpStream.Write(result, 0, result.Length);
            }else
            {
                udpClient.Send(result,result.Length);
            }

        }

        async Task<byte[]> ReadData(bool TCP = true)
        {
            waitingForSpecific.WaitOne();
            byte[] udpResult;
            byte[] lengthBytes = new byte[4];
            byte[] data = null;
            int length = 0;
            if (TCP)
            {
                await tcpStream.ReadAsync(lengthBytes, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBytes);
                }
                length = BitConverter.ToInt32(lengthBytes, 0);
                data = new byte[length];
                await tcpStream.ReadAsync(data, 0, length);
            }
            else
            {
                udpResult = (await udpClient.ReceiveAsync()).Buffer;
                Array.Copy(udpResult,4,data,0,udpResult.Length-4);
            }


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

            Connected = true;
            onConnectEvent.Invoke(w.LocalId);
            StartHandle();
        }



        // This needs to be exited after some kind of timeout
        public async Task Process()
        {
            adressOther = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            int portOther = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
            var udpEndpoint = new IPEndPoint(adressOther.Address,portOther+1);
            udpClient = new UdpClient(udpEndpoint);

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
            onConnectEvent.Invoke(w.LocalId);

            MainCaller.Do(()=> {

            //  Send missing players to new connectee
                for (int id = 0; id <4 && id != localid; id++ )
                {
                    Player player = PlayerManager.playerManager.players[id];
                    if(player !=null)
                    {
                        Packet packet = PlayerNetworkHandler.GenerateCreationRequest(player);
                        WritePacket(packet);
                    }

                }


                PlayerManager.playerManager.Add(localid, userName, true); 
            });

            StartHandle();
            
        }
        async void StartHandle()
        {
            Thread handle1Thread = new Thread(new ThreadStart(TcpHandle));
            Thread handle2Thread = new Thread(new ThreadStart(UdpHandle));
            handle1Thread.Start();
            handle2Thread.Start();
            await disconnected.Task;
        }

        void TcpHandle()
        {
            Task.Run(async () => { await HandlePackets(Tcp: true); });
        }

        void UdpHandle()
        {
            Task.Run(async () => { await HandlePackets(Tcp: false); });
        }

        public async Task HandlePackets(bool Tcp)
        {
            byte[] data;
            data = await ReadData(TCP: Tcp);


            if (!Connected)
            {
                return;
            }

            Packet p = General.Packet.Parser.ParseFrom(data);
            p.Sender = localid;
            Type packetType = Type.GetType(p.Type);


            Debug.Log("Received: " + p);

            UnityAction processPacket = null;
            if (packetType == typeof(MeDisconnect))
            {
                processPacket = () => {
                    Disconnect(respond: false); 
                };
            }else
            {
                processPacket = () =>
                {
                    NetworkHandler.UnMarshall(p);
                };
            }

            Thread handleThread = new Thread(new ThreadStart(processPacket));
            handleThread.IsBackground = true;
            handleThread.Start();
                //Processing needed

            await HandlePackets(Tcp);
        }
    }
}
