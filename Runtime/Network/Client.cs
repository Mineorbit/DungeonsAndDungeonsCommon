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

        private readonly int maxReceiveCount = 32;
        private readonly int maxSendCount = 32;


        public UnityEvent<int> onConnectEvent = new UnityEvent<int>();
        public UnityEvent onDisconnectEvent = new UnityEvent();

        public int Port;


        private bool realClosed;

        private IPEndPoint remote;
        public TcpClient tcpClient;
        public NetworkStream tcpStream;
        
        
        public UdpClient receivingUdpClient;

        public string userName;

        private readonly Semaphore waitingForTcp = new Semaphore(1, 1);
        private readonly Semaphore waitingForUdp = new Semaphore(1, 1);


        public Client(TcpClient tcpC, int lId, int port)
        {
            NetworkManager.allClients.Add(this);
            Connected = true;
            tcpClient = tcpC;
            receivingUdpClient = new UdpClient(port+1+lId);
            tcpStream = tcpClient.GetStream();
            localid = lId;
            var other = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address;
            remote = new IPEndPoint(other, port);
            Debug.Log("Finished Setup of Client");
        }

        public Client()
        {
            NetworkManager.allClients.Add(this);
        }


        public override string ToString()
        {
            return "NetworkClient " + userName + " " + localid;
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

            client.Connected = true;
            createThread.IsBackground = true;
            createThread.Start();


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
                Debug.Log("Client " + localid + " disconnected");
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
            client.tcpStream = client.tcpClient.GetStream();
        }

        private int writePort;
        private int readPort;
        public static void CreateUdpClientForClient(Client client, int writePort)
        {
            client.writePort = writePort;
            client.receivingUdpClient = new UdpClient(0);
            
            //client.readPort = Int32.Parse(client.receivingUdpClient.ToString());
            client.readPort = ((IPEndPoint) client.receivingUdpClient.Client.LocalEndPoint).Port;
            Debug.Log("This Client is receiving on "+client.readPort);
            client.remote = (IPEndPoint) client.tcpClient.Client.RemoteEndPoint;
            
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
                HandlePacket(p);
                handleCount--;
            }
        }

        private void HandlePacket(Packet p)
        {

            var packetType = Type.GetType(p.Type);


            Debug.Log("Going to Processing: " + p);

            UnityAction processPacket = () => { NetworkHandler.UnMarshall(p); };

            var handleThread = new Thread(new ThreadStart(processPacket));
            handleThread.IsBackground = true;
            handleThread.Start();
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


        public void WriteOut(PacketCarrier p, bool TCP = true)
        {
            var data = p.ToByteArray();
            var length = data.Length;
            var result = new byte[length + 4];
            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
                Array.Reverse(data);
            }

            Array.Copy(lengthBytes, 0, result, 0, 4);
            Array.Copy(data, 0, result, 4, length);
            
            UnityEngine.Debug.Log( "TCP: "+TCP +" Sending: " + p+ " over "+remote+" with "+localid);
            if (TCP)
            {
                tcpStream.Write(result, 0, result.Length);
            }
            else if (!TCP)
            {
                IPEndPoint r = new IPEndPoint(remote.Address,writePort);
                receivingUdpClient.Send(data, data.Length, r);
                Debug.Log("Send out UDP "+r+" "+data.Length);
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


        private async Task<byte[]> ReadData(bool TCP = true)
        {
            if (TCP)
                waitingForTcp.WaitOne();
            else
                waitingForUdp.WaitOne();
            byte[] udpResult;
            var lengthBytes = new byte[4];
            byte[] data = null;
            var length = 0;
            if (TCP)
            {
                await tcpStream.ReadAsync(lengthBytes, 0, 4);
                if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
                length = BitConverter.ToInt32(lengthBytes, 0);
                data = new byte[length];
                await tcpStream.ReadAsync(data, 0, length);
            }
            else
            {
                Debug.Log("Reading for UDP on " + remote);
                if(!isOnServer)
                {
                    udpResult = receivingUdpClient.Receive(ref remote);
                }
                else
                {
                    udpResult = receivingUdpClient.Receive(ref remote);
                }
                Debug.Log("Received something on udp of length "+udpResult.Length);
                data = new byte[udpResult.Length];
                Array.Copy(udpResult, 0, data, 0, udpResult.Length);
            }


            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            if (TCP)
                waitingForTcp.Release();
            else
                waitingForUdp.Release();
            return data;
        }

        // int tries; necessary?
        public async Task<T> ReadPacket<T>() where T : IMessage, new()
        {
            var data = await ReadData();

            var packetCarrier = PacketCarrier.Parser.ParseFrom(data);

            var p = packetCarrier.Packets[0];
            Debug.Log("Received " + p);
            T result;
            if (p.Content.TryUnpack(out result))
                return result;
            return await ReadPacket<T>();
        }


        public void Setup()
        {
            var w = Task.Run(ReadPacket<Welcome>).Result;

            localid = w.LocalId;

            Debug.Log("Received Welcome with "+localid+" and "+w.Udp);
            CreateUdpClientForClient(this,w.Udp);

            var meConnect = new MeConnect
            {
                Name = NetworkManager.userName,
                Udp = readPort
            };

            WritePacket(meConnect);

            Connected = true;
            onConnectEvent.Invoke(w.LocalId);


            Task.Run(async () => { await StartHandle(); });
        }


        // This needs to be exited after some kind of timeout
        public async Task Process()
        {
            
            Debug.Log("Processing");
            int port = ((IPEndPoint) receivingUdpClient.Client.LocalEndPoint).Port;

            Debug.Log("Test "+port);
            var w = new Welcome
            {
                LocalId = localid,
                Udp = port
            };

            Debug.Log("Receiving with "+port);
            //Send welcome

            WritePacket(w);

            var meConnect = await ReadPacket<MeConnect>();
            userName = meConnect.Name;
            writePort = meConnect.Udp;
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
            var handle2Thread = new Thread(UdpHandle);
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
            byte[] data;
            data = await ReadData(Tcp);


            var packetCarrier = PacketCarrier.Parser.ParseFrom(data);

            if (Connected)
                foreach (var packet in packetCarrier.Packets)
                    packetInBuffer.Enqueue(packet);

            //Processing needed

            await HandlePackets(Tcp);
        }
    }
}