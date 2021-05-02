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
            localid = lId;
        }

        public Client(TcpClient tcpC)
        {
            tcpClient = tcpC;
            localid = -1;
        }

        public static async Task<Client> Connect(IPAddress host, int port)
        {
            TcpClient tClient = new TcpClient();
            await tClient.ConnectAsync(host, port);

            Client client = new Client(tClient);

            await client.Setup();
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

            
            tcpStream.Write(result,0,result.Length);

        }

        public Welcome ReadWelcomePacket()
        {
            byte[] lengthBytes;
            byte[] data;
            tcpStream.Read(lengthBytes,0,4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }
            int length = BitConverter.ToInt32(lengthBytes,0);
            tcpStream.Read(data,0,length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return Welcome.Parser.ParseFrom(data);
        }


        public async Task Setup()
        {
            Welcome w = ReadWelcomePacket();
            Debug.Log(w);

        }

        // This needs to be exited when no more messages  are  received
        public async Task Process()
        {
            tcpStream = tcpClient.GetStream();

            Welcome w = new Welcome
            {
                LocalId = localid
            };

            //Send welcome

            WritePacket(w);

            while (tcpClient.Connected)
            {


            }
        }
    }
}
