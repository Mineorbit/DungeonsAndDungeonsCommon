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
        public static Queue<Packet> packetInBuffer = new Queue<Packet>();

        public static Queue<Packet> packetOutTCPBuffer = new Queue<Packet>();

        public static Queue<Packet> packetOutUDPBuffer = new Queue<Packet>();

        public bool Connected;

        private readonly TaskCompletionSource<bool> disconnected = new TaskCompletionSource<bool>();


        public bool isOnServer;
        public int localid;

        private readonly int maxReceiveCount = 15;
        private readonly int maxSendCount = 15;


        public UnityEvent<int> onConnectEvent = new UnityEvent<int>();
        public UnityEvent onDisconnectEvent = new UnityEvent();

        public int Port;


        private bool realClosed;

        private IPEndPoint remoteIPUdp;
        public TcpClient tcpClient;

        public NetworkStream tcpStream;
        public UdpClient udpClient;

        public string userName;

        private readonly Semaphore waitingForTcp = new Semaphore(1, 1);
        private readonly Semaphore waitingForUdp = new Semaphore(1, 1);


        public Client(TcpClient tcpC, UdpClient udpC, int lId, int port)
        {
            NetworkManager.allClients.Add(this);
            Connected = true;
            tcpClient = tcpC;
            udpClient = udpC;
            tcpStream = tcpClient.GetStream();
            localid = lId;
            var other = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address;
            remoteIPUdp = new IPEndPoint(other, port + 1 + lId);
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
            var client = new Client();

            client.Port = port;
            var createThread = new Thread(() =>
            {
                CreateTcpClientForClient(client, host, port);
                client.Connected = true;
                client.Setup();
            });

            createThread.IsBackground = true;
            createThread.Start();


            return client;
        }

        public void Disconnect(bool respond = true)
        {
            if (Connected)
            {
                if (respond)
                {
                    var meDisconnect = new MeDisconnect();
                    WritePacket(typeof(NetworkManagerHandler), meDisconnect);
                }

                UpdateOut();
                Debug.Log("Client " + localid + " disconnected");
                Connected = false;
            }
        }

        private void CloseConnection()
        {
            if (packetOutUDPBuffer.Count == 0 && packetOutTCPBuffer.Count == 0 && packetInBuffer.Count == 0 &&
                !realClosed)
            {
                tcpStream.Close();
                tcpClient.Close();
                udpClient.Close();
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
            var other = ((IPEndPoint) client.tcpClient.Client.RemoteEndPoint).Address;
        }

        public static void CreateUdpClientForClient(Client client)
        {
            client.udpClient = new UdpClient();
            client.remoteIPUdp = new IPEndPoint(((IPEndPoint) client.tcpClient.Client.RemoteEndPoint).Address,
                client.Port + 1 + client.localid);
            Debug.Log("Connecting UDP to: " + client.remoteIPUdp);


            client.udpClient.Connect(client.remoteIPUdp);
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

        private void UpdateOut()
        {
            var tcpSent = 0;
            var sendCount = maxSendCount;
            var tcpCarrier = new PacketCarrier();
            while (packetOutTCPBuffer.Count > 0 && sendCount > 0)
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
            while (packetOutUDPBuffer.Count > 0 && sendCount > 0)
            {
                var p = packetOutUDPBuffer.Dequeue();
                udpCarrier.Packets.Add(p);
                sendCount--;
                udpSent++;
            }

            //WriteOut(tcpCarrier,TCP: false);
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

            Debug.Log("Sent: " + p);
            if (TCP && tcpClient.Connected)
                tcpStream.Write(result, 0, result.Length);
            else if (!TCP) udpClient.Send(result, 0);
        }

        public void WritePacket(Packet p, bool TCP = true)
        {
            if (Connected)
            {
                p.Sender = localid;
                Debug.Log("Sending: " + p + " TCP: " + TCP);
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
                Debug.Log("Reading for UDP on " + remoteIPUdp);
                udpResult = udpClient.Receive(ref remoteIPUdp);
                Array.Copy(udpResult, 4, data, 0, udpResult.Length - 4);
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


            CreateUdpClientForClient(this);

            var meConnect = new MeConnect
            {
                Name = NetworkManager.userName
            };

            WritePacket(meConnect);

            Connected = true;
            onConnectEvent.Invoke(w.LocalId);


            Task.Run(async () => { await StartHandle(); });
        }


        // This needs to be exited after some kind of timeout
        public async Task Process()
        {
            var w = new Welcome
            {
                LocalId = localid
            };

            //Send welcome

            WritePacket(w);

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