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

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Client
    {
        public TcpClient tcpClient;
        public UdpClient udpClient;
        BinaryWriter ostream;

        int localid;

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
            TcpClient tClient = new TcpClient();
            await tClient.ConnectAsync(host, port);

            Client client = new Client(tClient);


            client.Setup();
            return client;
        }


        public void WritePacket(IMessage message)
        {
            byte[] data = message.ToByteArray();
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

            Debug.Log("Writing "+data.Length);
            
            tcpStream.Write(result,0,result.Length);

        }

        public T ReadPacket<T>() where T : IMessage, new()
        {
            byte[] lengthBytes = new byte[4];
            tcpStream.Read(lengthBytes,0,4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }
            int length = BitConverter.ToInt32(lengthBytes,0);
            byte[] data = new byte[length];
            tcpStream.Read(data,0,length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            var p = General.Packet.Parser.ParseFrom(data);
            T result;
            if (p.Content.TryUnpack<T>(out result))
            {
                return result;
            }else
            {
                throw new Exception();
            }

        }


        public void Setup()
        {
            Welcome w = ReadPacket<Welcome>();
            Debug.Log(w);

        }

        // This needs to be exited when no more messages  are  received
        public async Task Process()
        {

            Welcome w = new Welcome
            {
                LocalId = localid
            };

            //Send welcome

            WritePacket(w);

            PlayerConnect p = ReadPacket<PlayerConnect>();
            /*
            while (tcpClient.Connected)
            {


            }
            */
        }
    }
}
