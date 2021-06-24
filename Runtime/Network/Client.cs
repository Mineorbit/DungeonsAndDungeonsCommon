using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using General;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using State;
using UnityEngine;
using UnityEngine.Events;
using Type = System.Type;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Client
    {
        public Queue<Packet> packetInBuffer = new Queue<Packet>();

        public Queue<Packet> packetOutTCPBuffer = new Queue<Packet>();

        public Queue<Packet> packetOutUDPBuffer = new Queue<Packet>();

        public bool Connected;

        private readonly TaskCompletionSource<bool> disconnected = new TaskCompletionSource<bool>();


        public bool isOnServer;
        public int localid;

        private readonly int maxReceiveCount = 1;
        private readonly int maxSendCount = 1;


        public UnityEvent<int> onConnectEvent = new UnityEvent<int>();
        public UnityEvent onDisconnectEvent = new UnityEvent();

        public int Port;


        private bool realClosed;

        private IPEndPoint remote;
        public TcpClient tcpClient;
        public NetworkStream tcpStream;
        
        
        public UdpClient receivingUdpClient;

        public string userName;

        public IPAddress remoteAddress;

        private readonly Semaphore waitingForTcp = new Semaphore(1, 1);
        private readonly Semaphore waitingForUdp = new Semaphore(1, 1);


        public Client(TcpClient tcpC, int lId, int port)
        {
            NetworkManager.allClients.Add(this);
            Connected = true;
            tcpClient = tcpC;
            receivingUdpClient = new UdpClient(port+1+lId);
            //receivingUdpClient.AllowNatTraversal(true);
            tcpStream = tcpClient.GetStream();
            localid = lId;
            var other = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address;
            remote = new IPEndPoint(other, port);
        }

        public Client()
        {
            NetworkManager.allClients.Add(this);
        }


        public override string ToString()
        {
            return $"NetworkClient {localid} {userName}";
        }

        ~Client()
        {
            NetworkManager.allClients.Remove(this);
            Disconnect();
        }

        public static async Task<Client> Connect(IPAddress host, int port)
        {
            Client client = new Client {Port = port};

            var createThread = new Thread(() =>
            {
                CreateTcpClientForClient(client, host, port);
                client.Setup();
            });
            client.remoteAddress = host;
            client.Connected = true;
            createThread.IsBackground = true;
            createThread.Start();
            NetworkManager.threadPool.Add(createThread);
            return client;
        }

        public void Disconnect(bool respond = true)
        {
            if (Connected)
            {
                NetworkManager.allClients.Remove(this);
                if(NetworkManager.instance.client == this) NetworkManager.instance.client = null;
                if (respond)
                {
                    var meDisconnect = new MeDisconnect();
                    WritePacket(typeof(NetworkManagerHandler), meDisconnect);
                }
                UpdateOut(all: true);
                packetInBuffer.Clear();
                packetOutTCPBuffer.Clear();
                packetOutUDPBuffer.Clear();
                CloseConnection();
                Connected = false;
            }
        }

        private void CloseConnection()
        {
            if (packetOutUDPBuffer.Count == 0 && packetOutTCPBuffer.Count == 0 && packetInBuffer.Count == 0 && !realClosed)
            {
                tcpStream.Close();
                tcpClient.Close();
                receivingUdpClient.Close();
                onDisconnectEvent.Invoke();
                disconnected.SetResult(true);
                realClosed = true;
            }
        }

        //evtl async später
        public static void CreateTcpClientForClient(Client client, IPAddress host, int port)
        {
            client.tcpClient = new TcpClient(host.ToString(), port);
            client.tcpClient.SendTimeout = 1000;
            client.tcpClient.NoDelay = true;
            client.tcpStream = client.tcpClient.GetStream();
        }

        public static void CreateUdpClientForClient(Client client)
        {
            client.receivingUdpClient = new UdpClient();
            client.receivingUdpClient.Connect(client.remoteAddress,13565+1+client.localid);
            client.remote = new IPEndPoint(IPAddress.Any, 0);

        }

        public void FixedUpdate()
        {
            UpdateOut();
            UpdateIn();
            if (!Connected) CloseConnection();
        }

        private void UpdateIn()
        {
            var handleCount = maxReceiveCount;
            while (packetInBuffer.Count > 0 && handleCount > 0)
            {
                var p = packetInBuffer.Dequeue();
                GameConsole.Log("Handling "+p);
                HandlePacket(p);
                handleCount--;
            }
        }

        private void HandlePacket(Packet p)
        {
            UnityAction processPacket = () => { NetworkHandler.UnMarshall(p); };
            var handleThread = new Thread(new ThreadStart(processPacket));
            handleThread.IsBackground = true;
            handleThread.Start();
            NetworkManager.threadPool.Add(handleThread);
        }

        private void UpdateOut(bool all = false)
        {
            var tcpSent = 0;
            var sendCount = maxSendCount;
            var tcpCarrier = new PacketCarrier();
            while (packetOutTCPBuffer.Count > 0 && (all || sendCount > 0))
            {
                var p = packetOutTCPBuffer.Dequeue();
                tcpCarrier.Packets.Add(p);
                sendCount--;
                tcpSent++;
            }

            if (tcpSent > 0)
                WriteOut(tcpCarrier);

            var udpSent = 0;
            sendCount = maxSendCount;
            var udpCarrier = new PacketCarrier();
            while (packetOutUDPBuffer.Count > 0 && (all || sendCount > 0))
            {
                var p = packetOutUDPBuffer.Dequeue();
                udpCarrier.Packets.Add(p);
                sendCount--;
                udpSent++;
            }
            if(udpSent > 0)
                WriteOut(udpCarrier,TCP: false);
        }


        int tcpBufferSize;
        
        
        
        public void WriteOut(PacketCarrier p, bool TCP = true)
        {
            var data = p.ToByteArray();
            
            if (TCP || !NetworkManager.instance.useUDP)
            {
                GameConsole.Log($"-> TCP: "+p+$" {data.Length}");
                tcpStream.Write(data, 0, data.Length);
            }
            else
            {
                GameConsole.Log($"-> UDP: "+p+$" {data.Length} {remote.ToString()}");
                receivingUdpClient.Send(data, data.Length, remote);
            }
        }

        public void WritePacket(Packet p, bool TCP = true)
        {
            if (Connected)
            {
                p.Sender = localid;
                if (TCP)
                    packetOutTCPBuffer.Enqueue(p);
                else
                    packetOutUDPBuffer.Enqueue(p);
            }
        }

        public void WritePacket(IMessage message, bool TCP = true)
        {
            WritePacket(null, message, TCP);
        }

        public void WritePacket(Type handler, IMessage message, bool TCP = true)
        {
            Packet p = null;
            if (message.GetType() != typeof(Packet))
            {
                if (handler == null)
                    p = new Packet
                    {
                        Type = message.GetType().ToString(),
                        Content = Any.Pack(message)
                    };
                else
                    p = new Packet
                    {
                        Type = message.GetType().ToString(),
                        Content = Any.Pack(message),
                        Handler = handler.FullName
                    };
            }
            else
            {
                p = (Packet) message;
            }

            WritePacket(p, TCP);
        }

        private int tcpBufferLength = 8192;

        private async Task<byte[]> ReadData(bool TCP = true)
        {
            if (TCP)
            {
                waitingForTcp.WaitOne();
            }
            else
                waitingForUdp.WaitOne();
            byte[] data = null;
            if ( TCP)
            {
                byte[] tcpResult = new byte[tcpBufferLength];
                int readLength = tcpStream.Read(tcpResult, 0, tcpBufferLength);
                data = new byte[readLength];
                Array.Copy(tcpResult,data,readLength);
            }
            else
            {
                data = receivingUdpClient.Receive(ref remote);
            }

            if (TCP)
            {
                waitingForTcp.Release();
            }
            else
                waitingForUdp.Release();
            return data;
        }

        // int tries; necessary?
        public async Task<T> ReadPacket<T>() where T : IMessage, new()
        {
            var data = await ReadData();

            var packetCarrier = PacketCarrier.Parser.ParseFrom(data);
            GameConsole.Log("<- "+packetCarrier);
            
            foreach(var p in packetCarrier.Packets)
            {
                T result;
                if (p.Content.TryUnpack(out result))
                    return result;
            }
            return await ReadPacket<T>();
        }


        public void Setup()
        {
            var w = Task.Run(ReadPacket<Welcome>).Result;
            localid = w.LocalId;

            CreateUdpClientForClient(this);

            var welcomeUnstable = new WelcomeUnstable();
            welcomeUnstable.Message = "TESTMESSAGE";
            welcomeUnstable.UdpPort = ((IPEndPoint)receivingUdpClient.Client.LocalEndPoint).Port;
            
            // CLIENT SENDS FIRST UDP MESSAGE
            WritePacket(welcomeUnstable);
            
            var meConnect = new MeConnect
            {
                Name = NetworkManager.userName
            };

            WritePacket(meConnect,
                TCP: true);
            Connected = true;
            onConnectEvent.Invoke(w.LocalId);


            Task.Run(async () => { await StartHandle(); });
        }


        // This needs to be exited after some kind of timeout
        public async Task Process()
        {
            
            int port = ((IPEndPoint) receivingUdpClient.Client.LocalEndPoint).Port;
            var w = new Welcome
            {
                LocalId = localid
            };
            //Send welcome

            WritePacket(w);


            WelcomeUnstable  welcomeUnstable = await ReadPacket<WelcomeUnstable>();
            writePort = welcomeUnstable.UdpPort;
            var meConnect = await ReadPacket<MeConnect>();
            userName = meConnect.Name;
            Connected = true;
            onConnectEvent.Invoke(w.LocalId);
            
            MainCaller.Do(() =>
            {
                //  Send missing players to new connectee
                for (var id = 0; id < 4 && id != localid; id++)
                {
                    var player = PlayerManager.playerManager.players[id];
                    if (player != null)
                    {
                        var packet = PlayerNetworkHandler.GenerateCreationRequest(player);
                        WritePacket(packet);
                    }
                }


                PlayerManager.playerManager.Add(localid, userName, true);
            });

            await StartHandle();
        }

        private async Task StartHandle()
        {
            var handle1Thread = new Thread(TcpHandle);
            NetworkManager.threadPool.Add(handle1Thread);
            var handle2Thread = new Thread(UdpHandle);
            NetworkManager.threadPool.Add(handle2Thread);
            handle1Thread.Start();
            handle2Thread.Start();
            await disconnected.Task;
        }

        private void TcpHandle()
        {
            Task.Run(async () => { await HandlePackets(true); });
        }

        private void UdpHandle()
        {
            Task.Run(async () => { await HandlePackets(false); });
        }


        public async Task HandlePackets(bool Tcp)
        {
            while(true)
            {
            byte[] data;
            data = await ReadData(Tcp);
            
            // LENGTH IS 0 DISCONNECT

            if (data.Length == 0)
            {
                
            }

            var packetCarrier = PacketCarrier.Parser.ParseFrom(data);
            
            GameConsole.Log("<- "+packetCarrier+" "+data.Length);
            
            foreach (var packet in packetCarrier.Packets)
            {
                
                GameConsole.Log("\t| "+packet);
                packetInBuffer.Enqueue(packet);
            }
            //Processing needed
            }
        }
    }
}