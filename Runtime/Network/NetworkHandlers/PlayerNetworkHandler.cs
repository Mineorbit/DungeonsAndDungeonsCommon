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
            GetObservedPlayer().controller.enabled = NetworkManager.instance.localId != -1;
        }


        public Player GetObservedPlayer()
        {
            return (Player) observed;
        }
        
        
        public void UpdateInputData()
        {
            Message input = Message.Create(MessageSendMode.unreliable,(ushort) NetworkManager.ClientToServerId.playerInput);
            PlayerController c = (PlayerController) GetObservedPlayer().controller;
            input.AddVector3(c.movingDirection);
            input.AddVector3(c.targetDirection);
            input.AddVector3(c.forwardDirection);
            input.AddQuaternion(c.aimRotation);
            input.AddVector3(c.cameraForwardDirection);
            input.AddBool(c.movementInputOnFrame);
            input.AddBool(c.doInput);
            input.AddBool(c.takeInput);
            NetworkManager.instance.client.Send(input);
        }
        
        
        [MessageHandler((ushort)NetworkManager.ClientToServerId.playerInput)]
        public void PlayerInputProcessing(Message message)
        {
            GetObservedPlayer().movingDirection = message.GetVector3();
            GetObservedPlayer().targetDirection = message.GetVector3();
            GetObservedPlayer().forwardDirection = message.GetVector3();
            GetObservedPlayer().aimRotation = message.GetQuaternion();
            GetObservedPlayer().cameraForwardDirection = message.GetVector3();
            GetObservedPlayer().movementInputOnFrame = message.GetBool();
            GetObservedPlayer().doInput = message.GetBool();
            GetObservedPlayer().takeInput = message.GetBool();
        }
        

        public override void RequestCreation()
        {
           Player p = GetObservedPlayer();
           Message create = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.createPlayer);
            create.AddInt(p.localId);
            create.AddInt(p.Identity);
            create.AddString(p.name);
            create.AddVector3(p.transform.position);
            
            NetworkManager.instance.server.SendToAll(create);
        }
        
        
        public override void RequestRemoval()
        {
            Player p = GetObservedPlayer();
            Message remove = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.removeEntity);
            remove.AddInt(p.localId);
            NetworkManager.instance.server.SendToAll(remove);
        }
      


        [MessageHandler((ushort)NetworkManager.ServerToClientId.removePlayer)]
        public static void OnPlayerRemove(Message message)
        {
            int localIdToRemove = message.GetInt();
            
                MainCaller.Do(() => { PlayerManager.playerManager.Remove(localIdToRemove); });
            
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.createPlayer)]
        public static void OnPlayerCreate(Message message)
        {
            int localId = message.GetInt();
            int identity = message.GetInt();
            string name = message.GetString();
            Vector3 position = message.GetVector3();
                OnCreationRequest(identity, position, new Quaternion(0, 0, 0, 0), localId,
                    name);
        }


        

        public static void OnCreationRequest(int identity, Vector3 position, Quaternion rotation, int localId,
            string name)
        {

            MainCaller.Do(() =>
            {
				Util.Optional<int> id = new Util.Optional<int>();
				id.Set(identity);
                PlayerManager.playerManager.Add(localId, name, true, id);
                GameObject player = PlayerManager.playerManager.GetPlayer(localId);
                PlayerNetworkHandler h = player.GetComponent<PlayerNetworkHandler>();
                player.GetComponent<Player>().enabled = (NetworkManager.instance.localId == -1);
                h.enabled = true;
                h.owner = localId;

            });
        }
    }
}
