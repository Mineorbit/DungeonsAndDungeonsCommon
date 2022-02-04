using System;
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
            byte[] chunkData = m.GetBytes(isBigArray:true);
                MainCaller.Do(() =>
                {
                    ChunkManager.LoadChunk(DataToChunk(chunkData), false);
                });
        }


        private byte[] ChunkToData(ChunkData chunkData)
        {
            byte[] data = new byte[1024];
            foreach (LevelObjectInstanceData instanceData in chunkData.Data)
            {
                ushort elementType = (ushort) instanceData.Code;
                if (instanceData.Rot == 0)
                {
                    elementType = (ushort) (elementType & 0b_0011_1111_1111_1111);
                }
                if (instanceData.Rot == 1)
                {
                    elementType = (ushort) (elementType & 0b_0011_1111_1111_1111);
                    elementType = (ushort) (elementType | 0b_0100_0000_0000_0000);
                }
                if (instanceData.Rot == 2)
                {
                    elementType = (ushort) (elementType & 0b_0011_1111_1111_1111);
                    elementType = (ushort) (elementType | 0b_1000_0000_0000_0000);
                }
                if (instanceData.Rot == 3)
                {
                    elementType = (ushort) (elementType & 0b_0011_1111_1111_1111);
                    elementType = (ushort) (elementType | 0b_1100_0000_0000_0000);
                }
                GameConsole.Log($"Encode position {instanceData.X} {instanceData.Y} {instanceData.Z}");
                int z = 64*((int) instanceData.X) + 8*((int) instanceData.Y) +((int) instanceData.Z);
                int i = 2 * z;
                ushort number = Convert.ToUInt16(elementType);
                byte upper = (byte) (number >> 8);
                byte lower = (byte) (number & 0xff);
                data[i] = upper;
                data[i + 1] = lower;
            }
            return data;
        }

        private static ChunkData DataToChunk(byte[] data)
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
                        int rot = upper >> 6;
                        ushort elementType = BitConverter.ToUInt16(new byte[2] { upper, lower }, 0);
                        
                        elementType = (ushort) (elementType & 0b_0011_1111_1111_1111);
                        LevelObjectInstanceData objectData = new LevelObjectInstanceData();
                        objectData.Code = elementType;
                        objectData.X = (uint) i;
                        objectData.Y = (uint) j;
                        objectData.Z = (uint) k;
                        objectData.Rot = (uint) rot;
                        chunkData.Data.Add(objectData);
                        GameConsole.Log($"Got BlockData {objectData}");
                    }
            return chunkData;
        }
        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {
            Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
            message.Add(ChunkToData(chunkData),isBigArray:true);
            int id = ((LevelLoadTarget) GetObserved()).mover.target.gameObject.GetComponent<PlayerNetworkHandler>()
                .owner + 1;
            NetworkManager.instance.Server.Send(message,(ushort) id);
        }
        
        
        

        public override void SendAction(string actionName, ChunkData chunkData)
        {
            GameConsole.Log("CALLING REMOTE STREAM CHUNK");

            switch (actionName)
            {
                case "StreamChunkIntoCurrentLevelFrom":
                    StreamChunk(chunkData);
                    break;
                case "StreamChunkImmediateIntoCurrentLevelFrom":
                    StreamChunk(chunkData,immediate: true);
                    break;
                default:
                    base.SendAction(actionName, chunkData);
                    break;
            }
        }
    }
}
