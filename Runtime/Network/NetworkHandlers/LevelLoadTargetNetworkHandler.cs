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

        public void HandleChunkFragmentMessage(Message m)
        {
            
            int chunkX = m.GetInt();
            int chunkY = m.GetInt();
            int chunkZ = m.GetInt();
            int inChunkX = m.GetShort();
            int inChunkY = m.GetShort();
            int inChunkZ = m.GetShort();
            string fragment = $"{chunkX}|{chunkY}|{chunkZ}|{inChunkX}|{inChunkY}|{inChunkZ}";
            if (receivedChunkFragments.Contains(fragment))
            {
                GameConsole.Log($"ChunkFragment {fragment} was already loaded once");
                return;
            }
            receivedChunkFragments.Add(fragment);
            
            byte[] data = m.GetBytes(isBigArray: true);
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
        
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.streamChunk)]
        public static void OnStreamChunk(Message m)
        {{
            GameConsole.Log($"Received message of length: {m.UnreadLength}");
            receivedFragmentMessages.Enqueue(m);
        }

        private static Queue<Message> receivedFragmentMessages = new Queue<Message>();
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
            if (receivedFragmentMessages.Count > 0)
            {
                HandleChunkFragmentMessage(receivedFragmentMessages.Dequeue());
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
                        message.AddShort((byte) i);
                        message.AddShort((byte) j);
                        message.AddShort((byte) k);
                        byte[] data = new byte[1024];
                        for (int h = 0; h < 1024; h++)
                        {
                            data[h] = datas[i, j, k, h];
                        }
                        message.AddBytes(data,isBigArray:true);
                        //GameConsole.Log($"Bytes: {PrintByteArray(data)}");
                        messages.Add(message);
            
                    }

            int targetLocalId = ((LevelLoadTarget) observed).mover.target.GetComponent<Player>().localId + 1;
            foreach (Message m in messages)
            {
                GameConsole.Log($"Sent message of length: {m.WrittenLength}");
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
