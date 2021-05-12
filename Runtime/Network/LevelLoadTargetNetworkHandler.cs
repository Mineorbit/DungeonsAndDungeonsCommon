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
                MainCaller.Do(() => { ChunkData c = ChunkData.FromNetData(streamChunk.ChunkData);
                    Debug.Log("Received Chunk "+c);
                    ChunkManager.LoadChunk(c); });
            }
        }

        void StreamChunk(ActionParam chunkParam)
        {
            ChunkData toSend = (ChunkData)chunkParam.data;
            Debug.Log("Sending "+toSend);
            NetLevel.ChunkData netChunk = ChunkData.ToNetData(toSend);
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