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
            disabled_observed = observed.target.GetComponent<LevelObjectNetworkHandler>().disabled_observed;
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
                    Debug.Log("Received Chunk " + c);
                    ChunkManager.LoadChunk(c, false);
                });
        }

        private void StreamChunk(ActionParam chunkParam)
        {
            var toSend = (ChunkData) chunkParam.data;
            Debug.Log("Sending " + toSend);
            var netChunk = ChunkData.ToNetData(toSend);
            var streamChunk = new StreamChunk
            {
                ChunkData = netChunk
            };
            Marshall(streamChunk);
        }

        public override void SendAction(string actionName, ActionParam argument)
        {
            if (actionName == "StreamChunkIntoCurrentLevelFrom")
                StreamChunk(argument);
            else
                base.SendAction(actionName, argument);
        }
    }
}