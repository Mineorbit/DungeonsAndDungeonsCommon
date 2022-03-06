using System;
using Google.Protobuf.WellKnownTypes;
using RiptideNetworking;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerNetworkHandler : EntityNetworkHandler
    {
        
        public override void Awake()
        {
            base.Awake();
            observed = GetComponent<Player>();
            //GetObservedPlayer().controller.enabled = (NetworkManager.instance.localId != -1);
            GetObservedPlayer().controller.enabled = (NetworkManager.instance.localId == GetObservedPlayer().localId);
        }


        public Player GetObservedPlayer()
        {
            return (Player) observed;
        }
        
        
        public void UpdateInputData()
        {
            if(NetworkManager.instance.localId == GetObservedPlayer().localId)
            {
                //GameConsole.Log($"Updating Input Data for {GetObservedPlayer().localId}");
                Message input = Message.Create(MessageSendMode.unreliable,(ushort) NetworkManager.ClientToServerId.playerInput);
                PlayerController c = (PlayerController) GetObservedPlayer().controller;
                input.AddVector3(c.targetDirection);
                input.AddQuaternion(c.aimRotation);
                input.AddVector3(c.cameraForwardDirection);
                input.AddBool(c.movementInputOnFrame);
                input.AddBool(c.takeInput);
                NetworkManager.instance.Client.Send(input);
            }
        }
        
        
        [MessageHandler((ushort)NetworkManager.ClientToServerId.playerInput)]
        public static void PlayerInputProcessing(ushort id, Message message)
        {
            int localId = id - 1;
            Player player = PlayerManager.playerManager.players[localId];
            player.targetDirection = message.GetVector3();
            player.aimRotation = message.GetQuaternion();
            player.cameraForwardDirection = message.GetVector3();
            player.movementInputOnFrame = message.GetBool();
            player.takeInput = message.GetBool();
        }
        

        public override void RequestCreation()
        {
           Player p = GetObservedPlayer();
           Message create = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.createPlayer);
            create.AddInt(p.localId);
            create.AddInt(p.Identity);
            create.AddString(p.name);
            create.AddVector3(p.transform.position);
            
            NetworkManager.instance.Server.SendToAll(create);
            existsOn = new bool[] { true,true,true,true};
        }
        
        
        public override void RequestRemoval()
        {
            Player p = GetObservedPlayer();
            Message remove = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.removeEntity);
            remove.AddInt(p.localId);
            NetworkManager.instance.Server.SendToAll(remove);
        }
      


        [MessageHandler((ushort)NetworkManager.ServerToClientId.removePlayer)]
        public static void OnPlayerRemove(Message message)
        {
            int localIdToRemove = message.GetInt();
            PlayerManager.playerManager.Remove(localIdToRemove);
            
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.createPlayer)]
        public static void OnPlayerCreate(Message message)
        {
            int localId = message.GetInt();
            if (PlayerManager.playerManager.players[localId] != null)
                return;
            int identity = message.GetInt();
            string name = message.GetString();
            Vector3 position = message.GetVector3();
                OnCreationRequest(identity, position, new Quaternion(0, 0, 0, 0), localId,
                    name);
        }


        

        public static void OnCreationRequest(int identity, Vector3 position, Quaternion rotation, int localId,
            string name)
        {
				Util.Optional<int> id = new Util.Optional<int>();
				id.Set(identity);
                PlayerManager.playerManager.Add(localId, name, true, id);
                GameObject player = PlayerManager.playerManager.GetPlayer(localId);
                PlayerNetworkHandler h = player.GetComponent<PlayerNetworkHandler>();
                player.GetComponent<Player>().enabled = (NetworkManager.instance.localId == -1);
                h.enabled = true;
                h.owner = localId;
        }
    }
}
