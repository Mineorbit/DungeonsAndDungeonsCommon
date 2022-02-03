using Google.Protobuf;
using NetLevel;
using RiptideNetworking;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTargetNetworkHandler : LevelObjectNetworkHandler
    {

        public override void Awake()
        {
            disabled_observed = true;
            base.Awake();
        }
        
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.streamChunk)]
        public static void OnStreamChunk(Message m)
        {
            byte[] chunkData = m.GetBytes();
                MainCaller.Do(() =>
                {
                    ChunkManager.LoadChunk(ChunkData.Parser.ParseFrom(chunkData), false);
                });
        }


        private byte[] ChunkToData()
        {
            return null;
        }

        private ChunkData DataToChunk()
        {
            return null;
        }
        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {
            byte[] chunk = chunkData.ToByteArray();
            GameConsole.Log($"CHUNK SIZE: {chunk.Length}");
            Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
            message.AddBytes(chunk);
            NetworkManager.instance.Server.Send(message,(ushort) ((LevelLoadTarget)GetObserved()).mover.target.gameObject.GetComponent<PlayerNetworkHandler>().owner);
        }
        
        
        

        public override void SendAction(string actionName, ChunkData chunkData)
        {

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
