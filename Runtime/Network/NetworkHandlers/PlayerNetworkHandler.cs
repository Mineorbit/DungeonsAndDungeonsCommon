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
            Setup();
        }


        public Player GetObservedPlayer()
        {
            return (Player) observed;
        }
        
        public virtual void Setup()
        {
            if(observed != null && ((NetworkLevelObject)observed).identified)
                if(!isSetup)
                {   
                    // isOwner = !isOnServer && GetObservedPlayer().localId == NetworkManager.instance.localId;
                    GameConsole.Log($"Setting up PlayerHandler with {((NetworkLevelObject)observed).Identity} and {GetObservedPlayer().localId} Is Owner: {IsOwner()}");
                    owner = GetObservedPlayer().localId;
                    GetObservedPlayer().controller.enabled = !isOnServer && IsOwner();
                    if (isOnServer)
                    {
                        GetComponent<CharacterController>().enabled = false;
                    }
                    
                    isSetup = true;
                }
        }

        public override bool SendNecessary()
        {
            var pos = observed.transform.position;
            var rot = observed.transform.rotation;
            var aim = ((Entity) observed).aimRotation;
            var sendDist = (pos - lastSentPosition).magnitude;
            var sendRotAngle = Quaternion.Angle(rot, lastSentRotation);
            var sendAimRotAngle = Quaternion.Angle(aim, lastSentAimRotation);
            return sendDist > sendDistance || sendRotAngle > sendAngle || sendAimRotAngle > sendAngle;
        }

        


        public override void RequestCreation()
        {
            Packet p = GenerateCreationRequest(GetObservedPlayer());

            Server.instance.WriteAll(p);
        }

        [PacketBinding.Binding]
        public override void ProcessAction(Packet p)
        {
            if (isOnServer) Server.instance.WriteToAllExcept(p, GetObservedPlayer().localId);

            base.ProcessAction(p);
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

            var packet = new Packet
            {
                Type = typeof(PlayerRemove).FullName,
                Handler = typeof(PlayerNetworkHandler).FullName,
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
            var packet = new Packet
            {
                Type = typeof(PlayerCreate).FullName,
                Handler = typeof(PlayerNetworkHandler).FullName,
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
            Setup();
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
                h.enabled = true;
                h.Setup();

                if (isOnServer)
                    Destroy(h.GetObservedPlayer().controller);
            });
        }
    }
}
