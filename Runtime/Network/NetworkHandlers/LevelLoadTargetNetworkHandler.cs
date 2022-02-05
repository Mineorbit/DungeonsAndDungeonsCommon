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
            int i = m.GetShort();
            int j = m.GetShort();
            int k = m.GetShort();
            int a = m.GetInt();
            int b = m.GetInt();
            int c = m.GetInt();
            byte[] data = m.GetBytes(isBigArray: true);
            MainCaller.Do(() =>
            {
                List<LevelObjectInstanceData> instanceData = ChunkManager.BinaryToData(data,a,b,c,i*4,j*4,k*4);
                Vector3 offset = new Vector3(a * 8, b * 8, c * 8);
                foreach(LevelObjectInstanceData d  in instanceData)
                {
                    GameConsole.Log($"Resolved {d}");
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
                        
                        message.AddShort((byte) i);
                        message.AddShort((byte) j);
                        message.AddShort((byte) k);
                        message.AddInt(pos.Item1);
                        message.AddInt(pos.Item2);
                        message.AddInt(pos.Item3);
                        byte[] data = ChunkManager.DataToBinary(chunkData,i*4,j*4,k*4);
                        message.Add(data,isBigArray:true);
                        int id = ((LevelLoadTarget) GetObserved()).mover.target.gameObject.GetComponent<PlayerNetworkHandler>()
                            .owner + 1;
                        GameConsole.Log($"Sending message of length {message.WrittenLength}");
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
