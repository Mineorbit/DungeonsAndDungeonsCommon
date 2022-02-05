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
            ChunkData chunkData = DataToChunk(data,ChunkManager.GetChunkGridByID(chunkID));
            chunkData.ChunkId = chunkID;
            ChunkManager.LoadChunk(chunkData, false);
        }

        private byte[] ChunkToData(ChunkData chunkData, Tuple<int, int, int> gridPosition)
        {
            byte[] data = new byte[1024];
            GameConsole.Log($"{chunkData}");
        foreach (LevelObjectInstanceData instanceData in chunkData.Data)
            {
                ushort elementType = (ushort) instanceData.Code;

                int localX = ((int) instanceData.X)/(int)ChunkManager.storageMultiplier - 4;
                int localY = ((int) instanceData.Y)/(int)ChunkManager.storageMultiplier - 4;
                int localZ = ((int) instanceData.Z)/(int)ChunkManager.storageMultiplier - 4;
                int z = 64*((int) localX) + 8*((int) localY) +((int) localZ);
                int i = 2 * z;
                byte upper = (byte) (elementType >> 8);
                byte lower = (byte) (elementType & 0xff);

                upper = (byte) ((byte) (upper & 0x3f) | (byte)((byte)instanceData.Rot << 6));
                
                data[i] = upper;
                data[i + 1] = lower;
            }
            return data;
        }

        private static ChunkData DataToChunk(byte[] data, Tuple<int, int, int> gridPosition)
        {
            ChunkData chunkData = new ChunkData();
            for(int i = 0;i < 8;i++)
                for(int j = 0;j<8;j++)
                    for (int k = 0; k < 8; k++)
                    {
                        int z = 64*((int) i) + 8*((int) j) +((int) k);
                        int d = 2 * z;
                        byte upper = data[d];
                        byte lower = data[d+1];
                        byte upper2 = (byte) ((byte) (upper & 0x3f));
                        ushort elementType = (ushort) ( ((int) upper2) * 256 + (int)lower);
                        if(elementType != 0)
                        { 
                            int rot = upper >> 6;
                            LevelObjectInstanceData objectData = new LevelObjectInstanceData();
                            objectData.Code = elementType;
                            objectData.X = (uint) ((i+4) *(int)ChunkManager.storageMultiplier);
                            objectData.Y = (uint) ((j+4) *(int)ChunkManager.storageMultiplier);
                            objectData.Z = (uint) ((k+4) *(int)ChunkManager.storageMultiplier);
                            objectData.Rot = (uint) rot;
                            chunkData.Data.Add(objectData);
                        }
                    }
            GameConsole.Log($"{chunkData}");
            return chunkData;
        }
        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {
            Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
            var pos = ChunkManager.GetChunkGridByID(chunkData.ChunkId);
            message.AddInt(pos.Item1);
            message.AddInt(pos.Item2);
            message.AddInt(pos.Item3);
            byte[] data = ChunkToData(chunkData,ChunkManager.GetChunkGridByID(chunkData.ChunkId));
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
