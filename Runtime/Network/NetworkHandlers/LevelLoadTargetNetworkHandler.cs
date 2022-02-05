using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using NetLevel;
using RiptideNetworking;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTargetNetworkHandler : EntityNetworkHandler
    {

        public override void Awake()
        {
            base.Awake();
            disabled_observed = true;
            base.Awake();
        }
        
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.streamChunk)]
        public static void OnStreamChunk(Message m)
        {
            int a = m.GetInt();
            int b = m.GetInt();
            int c = m.GetInt();
            string chunkID = ChunkManager.GetChunkID(new Tuple<int, int, int>(a, b, c));
            MainCaller.Do(() =>
            {
                byte[] data = new byte[1024];
                data = m.GetBytes(isBigArray: true);
                LoadChunk(chunkID, data);
            });
        }

        static void LoadChunk(string chunkID, byte[] data)
        {
            ChunkData chunkData = ChunkManager.BinaryToData(data,ChunkManager.GetChunkGridByID(chunkID));
            chunkData.ChunkId = chunkID;
            ChunkManager.LoadChunk(chunkData, false);
        }

        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {
            Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
            var pos = ChunkManager.GetChunkGridByID(chunkData.ChunkId);
            message.AddInt(pos.Item1);
            message.AddInt(pos.Item2);
            message.AddInt(pos.Item3);
            byte[] data = ChunkManager.DataToBinary(chunkData);
            message.Add(data,isBigArray:true);
            int id = ((LevelLoadTarget) GetObserved()).mover.target.gameObject.GetComponent<PlayerNetworkHandler>()
                .owner + 1;
           NetworkManager.instance.Server.Send(message,(ushort) id);
        }

        
        

        public override void SendAction(string actionName, ChunkData chunkData)
        {

            Action a;
            switch (actionName)
            {
                case "StreamChunkIntoCurrentLevelFrom":
                    a = () => StreamChunk(chunkData);
                    break;
                case "StreamChunkImmediateIntoCurrentLevelFrom":
                    a = () => StreamChunk(chunkData);
                    break;
                default:
                    a = () => base.SendAction(actionName, chunkData);
                    break;
            }
            a.Invoke();
        }
    }
}
