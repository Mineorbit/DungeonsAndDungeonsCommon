using System;
using Game;
using General;
using Google.Protobuf.WellKnownTypes;
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
        

        public override void RequestCreation()
        {
            Packet p = GenerateCreationRequest(GetObservedPlayer());

            Server.instance.WriteAll(p);
        }

        public static Vector3 toVector3(MVector v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Quaternion toQuaternion(MQuaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        
        public static MVector fromVector3(Vector3 v)
        {
            MVector mVector = new MVector();
            mVector.X = v.x;
            mVector.Y = v.y;
            mVector.Z = v.z;
            return mVector;
        }

        public static MQuaternion fromQuaternion(Quaternion q)
        {
            MQuaternion mQuaternion = new MQuaternion();
            mQuaternion.X = q.x;
            mQuaternion.Y = q.y;
            mQuaternion.Z = q.z;
            mQuaternion.W = q.w;
            return mQuaternion;
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
        
        [PacketBinding.Binding]
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

        [PacketBinding.Binding]
        public override void ProcessAction(Packet p)
        {
            // THIS NEEDS TO HAPPEN BEFORE, BEACUSE p CHANGES THE SENDER NUMBER
            base.ProcessAction(p);
            
            if (isOnServer) Server.instance.WriteToAllExcept(p, GetObservedPlayer().localId);
        }

        public override void RequestRemoval()
        {
            Packet p = GenerateRemovalRequest(GetObservedPlayer(), this);
            Server.instance.WriteAll(p);
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

        public static Packet GenerateCreationRequest(Player p)
        {
            var position = p.transform.position;
            var rotation = p.transform.rotation;

            var playerNetworkHandler = p.GetComponent<PlayerNetworkHandler>();

            var playerCreate = new PlayerCreate
            {
                Name = p.playerName,
                LocalId = p.localId,
                X = position.x,
                Y = position.y,
                Z = position.z,
                Identity = p.Identity
            };
            
            string[] fs = typeof(PlayerNetworkHandler).FullName.Split('.');
            
            var packet = new Packet
            {
                Type = typeof(PlayerCreate).FullName,
                Handler = fs[fs.Length-1],
                Content = Any.Pack(playerCreate)
            };
            return packet;
        }


        [PacketBinding.Binding]
        public static void OnPlayerRemove(Packet value)
        {
            PlayerRemove playerRemove;
            if (value.Content.TryUnpack(out playerRemove))
            {
                var localIdToRemove = playerRemove.LocalId;

                MainCaller.Do(() => { PlayerManager.playerManager.Remove(localIdToRemove); });
            }
        }

        [PacketBinding.Binding]
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

        public override void Update()
        {
            base.Update();
            //Setup();
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
