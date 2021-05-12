using Game;
using General;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTargetNetworkHandler : LevelObjectNetworkHandler
    {
        public new LevelLoadTarget observed;


        [PacketBinding.Binding]
        public static void OnStreamChunk(Packet p)
        {
            StreamChunk streamChunk;
            if(p.Content.TryUnpack<StreamChunk>(out streamChunk))
            {
                MainCaller.Do(() => { ChunkManager.LoadChunk(ChunkData.FromNetData(streamChunk.ChunkData)); });
            }
        }

        void StreamChunk(ActionParam chunkParam)
        {
            NetLevel.ChunkData netChunk = ChunkData.ToNetData((ChunkData) chunkParam.data);
            StreamChunk streamChunk = new StreamChunk
            {
                ChunkData = netChunk
            };
            Marshall(streamChunk);
        }

        public override void SendAction(string actionName, ActionParam argument)
        {
            if(actionName == "StreamChunkIntoCurrentLevelFrom")
            {
                StreamChunk(argument);
            }
            else
            {
                base.SendAction(actionName,argument);
            }
        }
    }
}