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
                    ChunkManager.LoadChunk(ChunkData.Parser.ParseFrom(chunkData), false);
                });
        }


        private byte[] ChunkToData(ChunkData chunkData)
        {
            byte[] data = new byte[1024];
            for(int i =  0;i < 8;i++)
                for(int j = 0; j < 8 ; j++)
                    for (int k = 0; k < 8; k++)
                    {
                    
                    }
            return data;
        }

        private ChunkData DataToChunk(byte[] data)
        {
            return null;
        }
        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {
            byte[] chunk = chunkData.ToByteArray();
            GameConsole.Log($"CHUNK SIZE: {chunk.Length}");
            Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
            message.Add(chunk,isBigArray:true);
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
