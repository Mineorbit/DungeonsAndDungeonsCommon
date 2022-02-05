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
            int i = m.GetByte();
            int j = m.GetByte();
            int k = m.GetByte();
            string chunkID = ChunkManager.GetChunkID(new Tuple<int, int, int>(a, b, c));
            MainCaller.Do(() =>
            {
                byte[] data = new byte[1024];
                data = m.GetBytes(isBigArray: true);
                List<LevelObjectInstanceData> instanceData = ChunkManager.BinaryToData(data,a,b,c,i,j,k);
                Vector3 offset = new Vector3(a * 8, b * 8, c * 8);
                foreach(LevelObjectInstanceData d  in instanceData)
                {
                    LevelManager.currentLevel.Add(ChunkManager.LevelObjectInstanceDataToLevelObjectInstance(d, offset));
                }
            });
        }

        

        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {
            
            for(int i = 0; i < 2;i++)
                for(int j = 0; j < 2;j++)
                    for(int k = 0; k < 2;k++)
                    {
                        Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
                        var pos = ChunkManager.GetChunkGridByID(chunkData.ChunkId);
                        message.AddInt(pos.Item1);
                        message.AddInt(pos.Item2);
                        message.AddInt(pos.Item3);
                        message.AddByte((byte) i);
                        message.AddByte((byte) j);
                        message.AddByte((byte) k);
                        byte[] data = ChunkManager.DataToBinary(chunkData,i*4,j*4,k*4);
                        message.Add(data,isBigArray:true);
                        int id = ((LevelLoadTarget) GetObserved()).mover.target.gameObject.GetComponent<PlayerNetworkHandler>()
                            .owner + 1;
                        NetworkManager.instance.Server.Send(message,(ushort) id);
                    }
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
