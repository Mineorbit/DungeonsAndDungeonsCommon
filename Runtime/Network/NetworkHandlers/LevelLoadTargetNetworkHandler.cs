using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using NetLevel;
using PlasticPipe.Server;
using RiptideNetworking;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelLoadTargetNetworkHandler : EntityNetworkHandler
    {
        public static bool existsOneLevelLoadTargetInClient;
        private int target;
        public override void Awake()
        {
            disabled_observed = true;
            base.Awake();
        }

        public override void Start()
        {
            base.Start();
            if (!NetworkManager.instance.isOnServer && existsOneLevelLoadTargetInClient)
            {
                enabled = false;
                return;
            }
            
            target = ((LevelLoadTarget) observed).mover.target.GetComponent<Player>().localId + 1;
            for(int i = 0;i<4;i++)
                existsOn[i] = false;
            existsOn[target - 1] = true;
            
            existsOneLevelLoadTargetInClient = true;
            Level.deleteLevelEvent.AddListener(ResetHandler);
        }

        public override void ResetHandler()
        {
            base.ResetHandler();
            receivedChunkFragments.Clear();
        }
        
        static List<String> receivedChunkFragments = new List<String>();

        public void HandleChunkFragmentMessage(ChunkFragmentData m)
        {
            int chunkX = m.x;
            int chunkY = m.y;
            int chunkZ = m.z;
            int inChunkX = m.inx;
            int inChunkY = m.iny;
            int inChunkZ = m.inz;
            byte[] data = m.data;
            string fragment = $"{chunkX}|{chunkY}|{chunkZ}|{inChunkX}|{inChunkY}|{inChunkZ}";
            GameConsole.Log($"Got Fragment {fragment}");
            if (receivedChunkFragments.Contains(fragment))
            {
                GameConsole.Log($"ChunkFragment {fragment} was already loaded once");
                return;
            }
            
            receivedChunkFragments.Add(fragment);
            
            Vector3 offset = new Vector3(chunkX * 8, chunkY * 8, chunkZ * 8);
            for(int x = 0;x<8;x++)
            for(int y = 0;y<8;y++)
            for (int z = 0; z < 8; z++)
            {
                int i = 128*x+16*y+2*z;
                byte upper = data[i];
                byte lower = data[i + 1];
                    
                byte upper2 = (byte) ((byte) (upper & 0x3f));
                ushort elementType = (ushort) ( ((int) upper2) * 256 + (int)lower);
                int rot = upper >> 6;
                int code = 256*upper2 + lower;
                if(code != 0){
                    Vector3 inSubPartOffset = new Vector3(4*inChunkX+ 0.5f*x,4*inChunkY+0.5f*y,4*inChunkZ+0.5f*z);
                    Vector3 pos = offset + inSubPartOffset;
                    Build b = new Build
                    {
                        code = code,
                        position = pos,
                        rotation = rot
                    };
                    toBuild.Enqueue(b);
                }
            }

        }


        public struct ChunkFragmentData
        {
            public int x;
            public int y;
            public int z;
            public int inx;
            public int iny;
            public int inz;

            public byte[] data;
        }
        
        private static Queue<ChunkFragmentData> receivedChunkDataFragments = new Queue<ChunkFragmentData>();

        [MessageHandler((ushort)NetworkManager.ServerToClientId.streamChunk)]
        public static void OnStreamChunk(Message m)
        {
            
            
            int chunkX = m.GetInt();
            int chunkY = m.GetInt();
            int chunkZ = m.GetInt();
            int inChunkX = m.GetByte();
            int inChunkY = m.GetByte();
            int inChunkZ = m.GetByte();
            byte[] dat = m.GetBytes(1024);
            ChunkFragmentData d = new ChunkFragmentData
            {
                x = chunkX,
                y = chunkY,
                z = chunkZ,
                inx = inChunkX,
                iny = inChunkY,
                inz = inChunkZ,
            };
            d.data = new byte[1024];
            dat.CopyTo(d.data,0);
            GameConsole.Log($"Received Fragment {d}");
            receivedChunkDataFragments.Enqueue(d);
        }

        private static Queue<Build> toBuild = new Queue<Build>();
        struct Build
        {
            public Vector3 position;
            public int code;
            public int rotation;
        }

        private int targetLocalId = 0;
        private static int amountOfFragments = 4;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //targetLocalId = ((LevelLoadTarget) GetObserved()).mover.target.GetComponent<Player>().localId;
            for(int i = 0; i < amountOfFragments; i++) 
                HandleFragments();
            BuildFromQueue();
        }

        public void HandleFragments()
        {
            if (receivedChunkFragments.Count > 0)
            {
                HandleChunkFragmentMessage(receivedChunkDataFragments.Dequeue());
            }
        }

        public void BuildFromQueue()
        {
            if (toBuild.Count > 0)
            {
                Build b = toBuild.Dequeue();
                LevelObjectData objectData = Level.GetLevelObjectData(b.code);
                LevelManager.currentLevel.Add(objectData,b.position,Quaternion.Euler(0,90*b.rotation,0), null);
            }
        }
        
        private void StreamChunk(ChunkData chunkData, bool immediate = false)
        {

            Chunk chunk = ChunkManager.GetChunkByID(chunkData.ChunkId);
            List<Message> messages = new List<Message>();
            byte[,,,] datas = new byte[2, 2, 2, 1024];
            
            foreach (LevelObject levelObject in chunk.GetComponentsInChildren<LevelObject>())
            {
                Vector3 pos = levelObject.transform.localPosition;
                int a = pos.x >= 4 ? 1 : 0;
                int b = pos.y >= 4 ? 1 : 0;
                int c = pos.z >= 4 ? 1 : 0;

                int x = Convert.ToInt32( 2f * pos.x);
                int y = Convert.ToInt32( 2f * pos.y);
                int z = Convert.ToInt32( 2f * pos.z);
                
                
                int t = 64 * (x - 8 * a) + 8 * (y - 8 * b) + (z - 8 * c);
                ushort code = (ushort) levelObject.levelObjectDataType;

                
                byte upper = (byte) (code >> 8);
                byte lower = (byte) (code & 0xff);
                int rot = (byte) Mathf.Floor(levelObject.transform.eulerAngles.y / 90);
                
                upper = (byte) ((byte) (upper & 0x3f) | (byte)((byte)rot << 6));

                
                
                //assigning upper byte
                datas[a,b,c,2*t] = upper;
                //assigning lower byte
                datas[a,b,c,2*t+1] = lower;
            }
            
            for(int i = 0; i < 2;i++)
                for(int j = 0; j < 2;j++)
                    for(int k = 0; k < 2;k++)
                    {
                        Message message = Message.Create(MessageSendMode.reliable,(ushort)NetworkManager.ServerToClientId.streamChunk);
                        var position = chunk.transform.position;
                        message.AddInt( (int)position.x / 8);
                        message.AddInt( (int)position.y / 8);
                        message.AddInt( (int)position.z / 8);
                        message.AddByte((byte) i);
                        message.AddByte((byte) j);
                        message.AddByte((byte) k);
                        byte[] data = new byte[1024];
                        for (int h = 0; h < 1024; h++)
                        {
                            data[h] = datas[i, j, k, h];
                           //message.AddByte(datas[i, j, k, h]);
                        }
                        message.AddBytes(data,false);
                        //GameConsole.Log($"Bytes: {PrintByteArray(data)}");
                        messages.Add(message);
            
                    }

            int targetLocalId = ((LevelLoadTarget) observed).mover.target.GetComponent<Player>().localId + 1;
            foreach (Message m in messages)
            {
                GameConsole.Log($"Sent message of length: {m.UnreadLength}");
                NetworkManager.instance.Server.Send(m,(ushort) targetLocalId);
            };
            
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
