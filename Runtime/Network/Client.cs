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
        BinaryWriter ostream;

        public int localid;

        Semaphore waitingForSpecific = new Semaphore(0,1);

        public NetworkStream tcpStream;

        public Client(TcpClient tcpC, int lId)
        {
            tcpClient = tcpC;
            tcpStream = tcpClient.GetStream();
            localid = lId;
        }

        public Client(TcpClient tcpC)
        {
            tcpClient = tcpC;
            tcpStream = tcpClient.GetStream();
            localid = -1;
        }

        public static async Task<Client> Connect(IPAddress host, int port)
        {
            Debug.Log("Trying to Connect");

            TcpClient tcpClient = null;
            tcpClient = new TcpClient(host.ToString(), port);
            tcpClient.SendTimeout = 1000;

            Debug.Log("Connected");

            Client client = new Client(tcpClient);


            client.Setup();
            return client;
        }


        public void WritePacket(IMessage message)
        {

            General.Packet p = new General.Packet
            {
                Type = "Default",
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(message)
            };
            

            byte[] data = p.ToByteArray();
            int length = data.Length;
            byte[] result = new byte[length + 4];
            byte[] lengthBytes = BitConverter.GetBytes(length);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
                Array.Reverse(data); 
            }
            Array.Copy(lengthBytes,0,result,0,4);
            Array.Copy(data, 0, result, 4, length);
            
            tcpStream.Write(result,0,result.Length);

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
            Debug.Log("Reading for "+typeof(T));
            byte[] data = await ReadData();

            var p = General.Packet.Parser.ParseFrom(data);
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
            
            Debug.Log(w);

            PlayerConnect playerConnect = new PlayerConnect
            {
                EntityIdentifier = "",
                Name = NetworkManager.userName,
                LocalId = localid
            };

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

            PlayerConnect playerConnect = await ReadPacket<PlayerConnect>();
            Debug.Log("New Player: "+playerConnect);
            await HandlePackets();
        }

        public async Task HandlePackets()
        {
            byte[] data = await ReadData();

            Debug.Log("Received new Packet");

            //Processing needed

            await HandlePackets();
        }
    }
}
