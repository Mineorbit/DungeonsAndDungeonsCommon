using Game;
using General;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTargetNetworkHandler : LevelObjectNetworkHandler
    {
        public new LevelLoadTarget observed;

        public override void Awake()
        {
            disabled_observed = true;
            base.Awake();
        }
        
        
        [PacketBinding.Binding]
        public static void OnStreamChunk(Packet p)
        {
            StreamChunk streamChunk;
            if (p.Content.TryUnpack(out streamChunk))
                MainCaller.Do(() =>
                {
                    var c = ChunkData.FromNetData(streamChunk.ChunkData);
                    Debug.Log("Received Chunk " + c.chunkId+" with "+c.levelObjects.Count);
                    ChunkManager.LoadChunk(c, streamChunk.Immediate);
                });
        }

        private void StreamChunk(ActionParam chunkParam, bool immediate = false)
        {
            var toSend = (ChunkData) chunkParam.data;
            Debug.Log("Sending " + toSend.chunkId);
            var netChunk = ChunkData.ToNetData(toSend);
            var streamChunk = new StreamChunk
            {
                ChunkData = netChunk,
                Immediate = immediate
            };
            Marshall(streamChunk);
        }

        public override void SendAction(string actionName, ActionParam argument)
        {
            switch (actionName)
            {
                case "StreamChunkIntoCurrentLevelFrom":
                    StreamChunk(argument);
                    break;
                case "StreamChunkImmediateIntoCurrentLevelFrom":
                    StreamChunk(argument,immediate: true);
                    break;
                default:
                    base.SendAction(actionName, argument);
                    break;
            }
        }
    }
}