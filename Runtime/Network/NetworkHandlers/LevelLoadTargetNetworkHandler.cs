using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                        Vector3 inChunkOffset = new Vector3(4*inChunkX,4*inChunkY,4*inChunkZ);
                        Vector3 inSubPartOffset = new Vector3(x,y,z);
                        Vector3 pos = offset + inChunkOffset + inSubPartOffset;
                        GameConsole.Log($" {chunkX} {chunkY} {chunkZ} , {inChunkX} {inChunkY} {inChunkZ} , {x} {y} {z} => {offset} {inChunkOffset} {inSubPartOffset}");
                        LevelManager.currentLevel.Add(objectData,pos,Quaternion.identity, null);
                        }
                    }
            
        }

        
        public static string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            return sb.ToString();
        }
        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {

            Chunk chunk = ChunkManager.GetChunkByID(chunkData.ChunkId);
            List<Message> messages = new List<Message>();
            byte[,,,] datas = new byte[2, 2, 2, 1024];
            foreach (Transform levelObject in chunk.transform)
            {
                Vector3 pos = levelObject.transform.localPosition;
                int a = pos.x >= 4 ? 1 : 0;
                int b = pos.y >= 4 ? 1 : 0;
                int c = pos.z >= 4 ? 1 : 0;
                int t = 128 * (int) (pos.x - a) + 16 * (int) (pos.y - b) + 2 * (int) (pos.z - c);
                int code =  levelObject.gameObject.GetComponent<LevelObject>().levelObjectDataType;
                
                //assigning upper byte
                datas[a,b,c,t] = 0;
                //assigning lower byte
                datas[a,b,c,t+1] = (byte) code;
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
                        byte[] data = new byte[1024];
                        for (int h = 0; h < 1024; h++)
                        {
                            data[h] = datas[i, j, k, h];
                        }
                        message.AddBytes(data,isBigArray:true);
                        GameConsole.Log($"Bytes: {PrintByteArray(data)}");
                        messages.Add(message);
            
                    }

           
            foreach (Message m in messages)
            {
                NetworkManager.instance.Server.SendToAll(m);
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
