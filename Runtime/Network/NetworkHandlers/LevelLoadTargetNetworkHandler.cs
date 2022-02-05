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
            int chunkX = m.GetInt();
            int chunkY = m.GetInt();
            int chunkZ = m.GetInt();
            int inChunkX = m.GetShort();
            int inChunkY = m.GetShort();
            int inChunkZ = m.GetShort();
            byte[] data = m.GetBytes(isBigArray: true);
            Vector3 offset = new Vector3(chunkX * 8, chunkY * 8, chunkZ * 8);
            for(int x = 0;x<8;x++)
                for(int y = 0;y<8;y++)
                    for (int z = 0; z < 8; z++)
                    {
                    int i = 128*x+16*y+2*z;
                    byte upper = data[i];
                    byte lower = data[i + 1];
                    int code = 256*upper + lower;
                        if(code != 0){
                            LevelObjectData objectData = Level.GetLevelObjectData(code);
                            Vector3 smallOffset = new Vector3(inChunkX*4 + (float)x/2,inChunkY*4+(float)y/2,inChunkZ*4+(float)z/2);
                            LevelManager.currentLevel.Add(objectData,offset+smallOffset,Quaternion.identity, null);
                        }
                    }
            
        }

        

        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {

            Chunk chunk = ChunkManager.GetChunkByID(chunkData.ChunkId);
            Message[,,] messages = new Message[2,2,2];
            byte[,,][] datas = new byte[2, 2, 2][];
            foreach (Transform levelObject in chunk.transform)
            {
                Vector3 pos = levelObject.transform.localPosition;
                int a = pos.x >= 4 ? 1 : 0;
                int b = pos.y >= 4 ? 1 : 0;
                int c = pos.z >= 4 ? 1 : 0;
                int t = 128 * (int) (pos.x - a) + 16 * (int) (pos.y - b) + 2 * (int) (pos.z - c);
                int code =  levelObject.gameObject.GetComponent<LevelObject>().levelObjectDataType;
                byte[] intBytes = BitConverter.GetBytes(code);
                //assigning upper byte
                datas[a, b, c][t] = intBytes[2];
                //assigning lower byte
                datas[a, b, c][t] = intBytes[3];
            }
            
            for(int i = 0; i < 2;i++)
                for(int j = 0; j < 2;j++)
                    for(int k = 0; k < 2;k++)
                    {
                        Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
                        message.AddInt( (int)chunk.transform.position.x / 8);
                        message.AddInt( (int)chunk.transform.position.y / 8);
                        message.AddInt( (int)chunk.transform.position.z / 8);
                        message.AddShort((byte) i);
                        message.AddShort((byte) j);
                        message.AddShort((byte) k);
                        message.AddBytes(datas[i,j,k]);
                        messages[i, j, k] = message;
            
                    }
            

            foreach (Message message in messages)
            {
                NetworkManager.instance.Server.SendToAll(message);
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
