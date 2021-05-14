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
using General;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Client
    {
        public TcpClient tcpClient;
        public UdpClient udpClient;


        public UnityEvent<int> onConnectEvent = new UnityEvent<int>();
        public UnityEvent onDisconnectEvent = new UnityEvent();


        public bool isOnServer;

        public bool Connected = false;

        public string userName;
        public int localid;

        public int Port;

        Semaphore waitingForTcp = new Semaphore(1,1);
        Semaphore waitingForUdp = new Semaphore(1, 1);

        IPEndPoint remoteIPUdp;

        public NetworkStream tcpStream;

        private TaskCompletionSource<bool> disconnected = new TaskCompletionSource<bool>();


        public static Queue<Packet> packetInBuffer = new Queue<Packet>();

        public static Queue<Packet> packetOutTCPBuffer = new Queue<Packet>();

        public static Queue<Packet> packetOutUDPBuffer = new Queue<Packet>();





        public override string ToString()
        {
            return "NetworkClient "+userName+" "+localid;
        }

        

        public Client(TcpClient tcpC,UdpClient udpC, int lId,int port)
        {
            NetworkManager.allClients.Add(this);
            Connected = true;
            tcpClient = tcpC;
            udpClient = udpC;
            tcpStream = tcpClient.GetStream();
            localid = lId;
            IPAddress other = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address;
            remoteIPUdp = new IPEndPoint(other, port+1+lId);
        }

        public Client()
        {
            NetworkManager.allClients.Add(this);
        }

        ~Client()
        {
            NetworkManager.allClients.Remove(this);
            Disconnect();
        }

        

        public static async Task<Client> Connect(IPAddress host, int port)
        {

                Client client = new Client();

                client.Port = port;
                Thread createThread = new Thread(new ThreadStart(() => {
                    CreateTcpClientForClient(client, host, port);
                    client.Connected = true;
                    client.Setup();
                }));

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

        //evtl async später
        public static void CreateTcpClientForClient(Client client,IPAddress host, int port)
        {
            client.tcpClient = new TcpClient(host.ToString(), port);
            client.tcpClient.SendTimeout = 1000;
            client.tcpStream = client.tcpClient.GetStream();
            IPAddress other = ((IPEndPoint)client.tcpClient.Client.RemoteEndPoint).Address;
            
        }

        public static void CreateUdpClientForClient(Client client)
        {
            client.udpClient = new UdpClient();
            client.remoteIPUdp = new IPEndPoint(((IPEndPoint) client.tcpClient.Client.RemoteEndPoint).Address, client.Port+1+client.localid);
            Debug.Log("Connecting UDP to: "+ client.remoteIPUdp);
            
            
            client.udpClient.Connect(client.remoteIPUdp);
        }

        int maxReceiveCount = 15;
        int maxSendCount = 15;
        public void FixedUpdate()
        {
            UpdateOut();
            UpdateIn();
        }

        void UpdateIn()
        {
            int handleCount = maxReceiveCount;

            while(packetInBuffer.Count>0 && handleCount>0)
            {
                Packet p = packetInBuffer.Dequeue();
                HandlePacket(p);
                handleCount--;
            }


        }

        void HandlePacket(Packet p)
        {
            Type packetType = Type.GetType(p.Type);


            Debug.Log("Going to Processing: " + p);

            UnityAction processPacket = null;
            if (packetType == typeof(MeDisconnect))
            {
                processPacket = () => {
                    Disconnect(respond: false);
                };
            }
            else
            {
                processPacket = () =>
                {
                    NetworkHandler.UnMarshall(p);
                };
            }

            Thread handleThread = new Thread(new ThreadStart(processPacket));
            handleThread.IsBackground = true;
            handleThread.Start();
        }

        void UpdateOut()
        {
            int tcpSent = 0;
            int sendCount = maxSendCount;
            General.PacketCarrier tcpCarrier = new PacketCarrier();
            while (packetOutTCPBuffer.Count > 0 && sendCount > 0)
            {
                Packet p = packetOutTCPBuffer.Dequeue();
                tcpCarrier.Packets.Add(p);
                sendCount--;
                tcpSent++;
            }
            if(tcpSent > 0)
            WriteOut(tcpCarrier);

            int udpSent = 0;
            sendCount = maxSendCount;
            General.PacketCarrier udpCarrier = new PacketCarrier();
            while (packetOutUDPBuffer.Count > 0 && sendCount > 0)
            {
                Packet p = packetOutUDPBuffer.Dequeue();
                udpCarrier.Packets.Add(p);
                sendCount--;
                udpSent++;
            }

            //WriteOut(tcpCarrier,TCP: false);
        }



        public void WriteOut(PacketCarrier p, bool TCP = true)
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

            Debug.Log("Sent: " + p);
            if (TCP && tcpClient.Connected)
            {
                tcpStream.Write(result, 0, result.Length);
            }
            else if(!TCP)
            {
                Debug.Log("Over " + udpClient);
                udpClient.Send(result, 0);
            }

        }

        public void WritePacket(Packet p, bool TCP = true)
        {
            p.Sender = localid;
            UnityEngine.Debug.Log("Sending: "+p+" TCP: "+TCP);
            if(TCP)
            {
                packetOutTCPBuffer.Enqueue(p);
            }
            else
            {
                packetOutUDPBuffer.Enqueue(p);
            }
        }

        public void WritePacket(IMessage message,bool TCP = true)
        {
            General.Packet p = null;
            if (message.GetType() != typeof(Packet))
            { 
                p = new General.Packet
                {
                Type = message.GetType().ToString(),
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message)
                };
            }else
            {
                p = (Packet) message;
            }
            WritePacket(p,TCP: TCP);

        }

        

        async Task<byte[]> ReadData(bool TCP = true)
        {
            Debug.Log("Waiting for Data");
            if(TCP)
            {
                waitingForTcp.WaitOne();
            }else
            {
                waitingForUdp.WaitOne();
            }
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
                Debug.Log("Reading for UDP on "+remoteIPUdp);
                udpResult = udpClient.Receive(ref remoteIPUdp);
                Array.Copy(udpResult,4,data,0,udpResult.Length-4);
            }


            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            if(TCP)
            {
                waitingForTcp.Release();
            }
            else
            {
                waitingForUdp.Release();
            }
            return data;
        }

        // int tries; necessary?
        public async Task<T> ReadPacket<T>() where T : IMessage, new()
        {
            byte[] data = await ReadData();

            PacketCarrier packetCarrier = General.PacketCarrier.Parser.ParseFrom(data);

            Packet p = packetCarrier.Packets[0];
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


            CreateUdpClientForClient(this);

            MeConnect meConnect = new MeConnect
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
           

            Welcome w = new Welcome
            {
                LocalId = localid
            };

            //Send welcome

            WritePacket(w);

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

            await StartHandle();
            
        }
        async Task StartHandle()
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



            PacketCarrier packetCarrier = General.PacketCarrier.Parser.ParseFrom(data);
            
            foreach( Packet packet in packetCarrier.Packets)
            {
                packetInBuffer.Enqueue(packet);
            }
            
            //Processing needed

            await HandlePackets(Tcp);
        }
    }
}
