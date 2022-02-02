using System;
using Game;
using General;
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
            PlayerController c = (PlayerController) GetObservedPlayer().controller;
            PlayerInputUpdate playerInputUpdate = new PlayerInputUpdate();
            playerInputUpdate.MovingDirection = fromVector3(c.movingDirection);
            playerInputUpdate.TargetDirection = fromVector3(c.targetDirection);
            playerInputUpdate.ForwardDirection = fromVector3(c.forwardDirection);
            playerInputUpdate.AimRotation = fromQuaternion(c.aimRotation);
            playerInputUpdate.CameraForwardDirection = fromVector3(c.cameraForwardDirection);
            playerInputUpdate.MovementInputOnFrame = c.movementInputOnFrame;
            playerInputUpdate.DoInput = c.doInput;
            playerInputUpdate.TakeInput = c.takeInput;
            Marshall(((NetworkLevelObject) observed).Identity,playerInputUpdate, TCP: false,true);
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
        
        
       
        
        [MessageHandler((ushort)NetworkManager.ClientToServerId.playerInput)]
        public void PlayerInputProcessing(Packet p)
        {
            PlayerInputUpdate playerInputUpdate;
            if (p.Content.TryUnpack(out playerInputUpdate))
            {
                GetObservedPlayer().movingDirection = toVector3(playerInputUpdate.MovingDirection);
                GetObservedPlayer().targetDirection = toVector3(playerInputUpdate.TargetDirection);
                GetObservedPlayer().forwardDirection = toVector3(playerInputUpdate.ForwardDirection);
                GetObservedPlayer().aimRotation = toQuaternion(playerInputUpdate.AimRotation);
                GetObservedPlayer().cameraForwardDirection = toVector3(playerInputUpdate.CameraForwardDirection);
                GetObservedPlayer().movementInputOnFrame = playerInputUpdate.MovementInputOnFrame;
                GetObservedPlayer().doInput = playerInputUpdate.DoInput;
                GetObservedPlayer().takeInput = playerInputUpdate.TakeInput;
            }
        }

        

        

        public static Packet GenerateRemovalRequest(Player p, PlayerNetworkHandler playerNetworkHandler)
        {
            var playerRemove = new PlayerRemove
            {
                LocalId = p.localId
            };
            
            string[] fs = typeof(PlayerNetworkHandler).FullName.Split('.');
            
            var packet = new Packet
            {
                Type = typeof(PlayerCreate).FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(playerRemove),
                Identity = p.Identity
            };
            return packet;
        }

      


        [MessageHandler((ushort)NetworkManager.ServerToClientId.removePlayer)]
        public static void OnPlayerRemove(Packet value)
        {
            PlayerRemove playerRemove;
            if (value.Content.TryUnpack(out playerRemove))
            {
                var localIdToRemove = playerRemove.LocalId;

                MainCaller.Do(() => { PlayerManager.playerManager.Remove(localIdToRemove); });
            }
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.createPlayer)]
        public static void OnPlayerCreate(Packet value)
        {
            PlayerCreate playerCreate;
            if (value.Content.TryUnpack(out playerCreate))
            {
                var position = new Vector3(playerCreate.X, playerCreate.Y, playerCreate.Z);
                OnCreationRequest(playerCreate.Identity, position, new Quaternion(0, 0, 0, 0), playerCreate.LocalId,
                    playerCreate.Name);
            }
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
