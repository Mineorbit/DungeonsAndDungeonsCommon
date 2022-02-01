using System;
using System.Collections.Generic;
using System.Linq;
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


        public static int sentPacketCarriers;
        public static int receivedPacketCarriers;
        public static int handledPackets;

        public int lastSentPacketCarrier = 0;
        public int lastReceivedPacketCarrier = 0;


        private int maxTcpPackSize = 4*8192;
        private int maxUdpPackSize = 8*8192;

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


        // SERVER
        public Client(TcpClient tcpC, int lId, int port)
        {
            NetworkManager.allClients.Add(this);
            Connected = true;
            tcpClient = tcpC;
            receivingUdpClient = new UdpClient(port+1+lId);
            //receivingUdpClient.AllowNatTraversal(true);
            tcpStream = tcpClient.GetStream();
            localid = lId;
            remote = new IPEndPoint(IPAddress.Any, 0);
            udpRemote = (IPEndPoint) receivingUdpClient.Client.RemoteEndPoint;
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

        public static Client Connect(IPAddress host, int port)
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
                    Packet packet = new Packet
            		{
                	Type = meDisconnect.GetType().FullName,
                	Handler = typeof(NetworkManagerHandler).FullName,
                		Content = Any.Pack(meDisconnect)
            		};
                    WritePacket(packet);
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
            // client.tcpClient.SendTimeout = 1000;
            client.tcpStream = client.tcpClient.GetStream();
        }

        private IPEndPoint remoteAccess;
        private IPEndPoint udpRemote;
        public static void CreateUdpClientForClient(Client client)
        {
            client.receivingUdpClient = new UdpClient();
            client.receivingUdpClient.Connect(client.remoteAddress,13565+1+client.localid);
            client.remote = new IPEndPoint(IPAddress.Any, 0);
        }

        public void FixedUpdate()
        {
            UpdateOut();
            if (!Connected) CloseConnection();
        }

        
        private void HandlePacket(Packet p)
        {
            handledPackets++;
            
                UnityAction processPacket = () => { NetworkHandler.UnMarshall(p); };
                processPacket.Invoke();
        }

        private void UpdateOut(bool all = false)
        {
            var tcpSent = 0;
            var tcpCarrier = new PacketCarrier();
            while (packetOutTCPBuffer.Count > 0 && ( tcpCarrier.CalculateSize() < maxTcpPackSize))
            {
                var p = packetOutTCPBuffer.Dequeue();
                var oldCarrier = tcpCarrier.Clone();
                tcpCarrier.Packets.Add(p);
                if (tcpCarrier.CalculateSize() > maxTcpPackSize)
                {
                    tcpCarrier = oldCarrier;
                    packetOutTCPBuffer.Enqueue(p);
                    break;
                }
                tcpSent++;
            }

            if (tcpSent > 0)
                WriteOut(tcpCarrier);

            var udpSent = 0;
            var udpCarrier = new PacketCarrier();
            while (packetOutUDPBuffer.Count > 0 && ( udpCarrier.CalculateSize() < maxUdpPackSize))
            {
                var p = packetOutUDPBuffer.Dequeue();
                var oldCarrier = udpCarrier.Clone();
                udpCarrier.Packets.Add(p);
                if (udpCarrier.CalculateSize() > maxUdpPackSize)
                {
                    udpCarrier = oldCarrier;
                    packetOutUDPBuffer.Enqueue(p);
                    break;
                }
                udpSent++;
                
            }
            if(udpSent > 0)
                WriteOut(udpCarrier,TCP: false);
            
        }


        int tcpBufferSize;

        private bool debugNetwork = false;
        
        public void WriteOut(PacketCarrier p, bool TCP = true)
        {
            var data = p.ToByteArray();
            sentPacketCarriers++;
            if (debugNetwork)
            {
                string r = TCP ? "TCP" : "UDP";
                GameConsole.Log($"Sending {p} to {remote} {data.Length} bytes via {r}");
            }
            if (TCP || !NetworkManager.instance.useUDP)
            {
                tcpStream.Write(data, 0, data.Length);
            }
            else
            {
                if(isOnServer)
                {
                    try
                    {
                        // ADDRESS SOMEHOW NOT WORKING
                        receivingUdpClient.Send(data, data.Length, remote);
                    }
                    catch (
                        Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    //receivingUdpClient.Send(data, data.Length);
                }
                else
                {
                    receivingUdpClient.Send(data, data.Length);
                }
            }
        }

        public void WritePacket(Packet p, bool TCP = true, bool overrideSame = false)
        {
            if (Connected)
            {
                p.Sender = NetworkManager.instance.localId;
                if(TCP)
                    GameConsole.Log($"Sending {localid} {p}");
                if (TCP)
                {
                    if (overrideSame)
                    {
                        packetOutTCPBuffer =
                            new Queue<Packet>(packetOutTCPBuffer.Where((x) => (x.Identity != p.Identity) && (x.Type != p.Type)));
                    }
                    packetOutTCPBuffer.Enqueue(p);
                }
                else
                {
                    if (overrideSame)
                    {
                        packetOutUDPBuffer =
                            new Queue<Packet>(packetOutUDPBuffer.Where((x) => (x.Identity != p.Identity) && (x.Type != p.Type)));
                    }
                    packetOutUDPBuffer.Enqueue(p);
                }
                
            }
        }


        private byte[] ReadData(bool TCP = true)
        {
            byte[] data = null;
            if (TCP)
            {
                int tcpBufferLength = maxTcpPackSize*2;
                byte[] tcpResult = new byte[tcpBufferLength];
                int readLength = tcpStream.Read(tcpResult, 0, tcpBufferLength);
                data = new byte[readLength];
                // GameConsole.Log($"Received:  {readLength} bytes");
                Array.Copy(tcpResult,data,readLength);
            }
            else
            {
                data = receivingUdpClient.Receive(ref remote);
            }
            return data;
        }

        // int tries; necessary?
        public async Task<T> ReadPacket<T>() where T : IMessage, new()
        {
            var data = ReadData();

            var packetCarrier = PacketCarrier.Parser.ParseFrom(data);
            
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

            
            var meConnect = new MeConnect
            {
                Name = NetworkManager.userName
            };
            Packet packet = new Packet
            {
                Type = meConnect.GetType().FullName,
                Content = Any.Pack(meConnect)
            };
            WritePacket(packet, TCP: true);
            Connected = true;
            onConnectEvent.Invoke(w.LocalId);


            Task.Run(async () => { await StartHandle(); });
        }


        // This needs to be exited after some kind of timeout
        public async Task Process()
        {
            
            int port = ((IPEndPoint) receivingUdpClient.Client.LocalEndPoint).Port;
            
            //receivingUdpClient.Connect(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address,port+1+localid);
            var w = new Welcome
            {
                LocalId = localid
            };
            //Send welcome
            Packet packet = new Packet
            {
                Type = w.GetType().FullName,
                Content = Any.Pack(w)
            };
            WritePacket(packet);


            // WelcomeUnstable  welcomeUnstable = await ReadPacket<WelcomeUnstable>();
            // GameConsole.Log($"WELCOME: {welcomeUnstable}");
            
            var meConnect = await ReadPacket<MeConnect>();
            userName = meConnect.Name;
            Connected = true;
            onConnectEvent.Invoke(w.LocalId);
            
            MainCaller.Do(() =>
            {
                // THIS SHOULD BE REWORKED SUCH THAT EACH ENTITY NETWORKHANDLER ON CONNECTION OF A NEW PLAYER SENDS ITS
                // OWN CREATION REQUEST
                //  Send all current players to new connectee
                for (var id = 0; id < 4 && id != localid; id++)
                {
                    var player = PlayerManager.playerManager.players[id];
                    if (player != null)
                    {
                        Packet packet = PlayerNetworkHandler.GenerateCreationRequest(player);
                        WritePacket(packet);
                    }
                }


                PlayerManager.playerManager.Add(localid, userName, true,null);
                Server.onConnectEvent.Invoke(localid);
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

        // Maybe refactor more here
        private void TcpHandle()
        {
            HandlePackets(true);
        }

        private void UdpHandle()
        {
            HandlePackets(false);
        }


        public void HandlePackets(bool Tcp)
        {
            while(true)
            {
                byte[] data;
                data = ReadData(Tcp);
                receivedPacketCarriers++;
            // LENGTH IS 0 DISCONNECT

            if (data.Length != 0)
            {
                var packetCarrier = PacketCarrier.Parser.ParseFrom(data);
            
                foreach (var packet in packetCarrier.Packets)
                {
                    //GameConsole.Log($"Processing: {packet}");
                    HandlePacket(packet);
                }
            }
            //Processing needed
            }
        }
    }
}
