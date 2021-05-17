using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using General;
using Google.Protobuf;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Server
    {
        public static Server instance;
        public Client[] clients;

        private TcpListener listener;

        private readonly IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        private readonly int port = 13565;


        public Server()
        {
            if (instance == null) instance = this;
        }


        public override string ToString()
        {
            var s = "Server:";
            for (var i = 0; i < 4; i++) s += "\n" + i + ": " + clients[i];
            return s;
        }


        public void Start()
        {
            clients = new Client[4];
            listener = new TcpListener(localAddr, port);
            listener.Start();
            var createThread = new Thread(() => { Task.Run(HandleNewConnection); });
            createThread.IsBackground = true;
            createThread.Start();
        }

        private async Task HandleNewConnection()
        {
            var tpClient = await listener.AcceptTcpClientAsync();
            var i = GetFreeSlot();
            //int udpPort = port + 1 + i;
            //UdpClient udClient = new UdpClient(udpPort);

            var udClient = new UdpClient();
            if (i == -1)
            {
                tpClient.Close();
            }
            else
            {
                var c = new Client(tpClient, udClient, i, port);
                clients[i] = c;
                var handleThread = new Thread(() =>
                {
                    Task.Run(async () =>
                    {
                        await c.Process();
                        c.Disconnect();
                    });
                });
                handleThread.IsBackground = true;
                handleThread.Start();
            }

            await HandleNewConnection();
        }

        private int GetFreeSlot()
        {
            for (var i = 0; i < 4; i++)
                if (clients[i] == null)
                    return i;
            return -1;
        }


        public void Stop()
        {
            DisconnectAll();
            StopListen();
        }

        public void DisconnectAll()
        {
            for (var i = 0; i < 4; i++) Disconnect(i);
        }

        public void Disconnect(int localId)
        {
            if (clients[localId] != null)
            {
                clients[localId].Disconnect();
                clients[localId] = null;
            }
        }


        public void WriteAll(Packet p)
        {
            for (var i = 0; i < 4; i++)
                if (clients[i] != null)
                    clients[i].WritePacket(p);
        }

        public void WriteAll(IMessage message, bool TCP = true)
        {
            for (var i = 0; i < 4; i++)
                if (clients[i] != null)
                    clients[i].WritePacket(message, TCP);
        }

        public void WriteAll(IMessage message, int except, bool TCP = true)
        {
            for (var i = 0; i < 4 && i != except; i++)
                if (clients[i] != null)
                    clients[i].WritePacket(message, TCP);
        }

        public void StopListen()
        {
            listener.Stop();
        }
    }
}