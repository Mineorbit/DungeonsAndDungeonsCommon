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
        static int maxSize = 4096;
        byte[] dataOut = new byte[maxSize];
        //BinaryWriter ostream = new BinaryWriter(new MemoryStream(dataOut));

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


        //Temporary stolen from stackoverflow
        public void WritePacket(IMessage message)
        {
            /*
            CodedOutputStream output = new CodedOutputStream(ostream.BaseStream, true);
            output.WriteMessage(message);
            output.Flush();
            (ostream.BaseStream as MemoryStream).SetLength(0); // reset stream for next packet(s)

            tcpStream.Write(dataOut,0, message.CalculateSize() + 1);

            */

            message.WriteDelimitedTo(tcpStream);

        }


        public async Task Setup()
        {
            Welcome w = Welcome.Parser.ParseDelimitedFrom(tcpStream);

        }

        // This needs to be exited when no more messages  are  received
        public async Task Process()
        {
            tcpStream = tcpClient.GetStream();

            Welcome w = new Welcome
            {
                LocalId = localid
            };
            WritePacket(w);
            //Send welcome


        }
    }
}
