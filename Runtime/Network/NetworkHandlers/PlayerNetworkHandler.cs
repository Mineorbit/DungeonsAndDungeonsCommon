using System;
using Game;
using General;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerNetworkHandler : EntityNetworkHandler
    {
        private new Player observed;

        private string _identity;

        public override void OnIdentify()
        {
            base.OnIdentify();
            Setup();
        }
        
        public override void Awake()
        {
            base.Awake();
            observed = GetComponent<Player>();
        }

        private bool isSetup = false;
        public virtual void Setup()
        {
            if(observed != null && identified)
            if(!isSetup)
            { 
            isSetup = true;
            Debug.Log("Setting up PlayerHandler with "+Identity+" and "+observed.localId);
            isOwner = !isOnServer && observed.localId == NetworkManager.instance.localId;
            owner = observed.localId;
            
            GetComponent<PlayerController>().enabled = !isOnServer && isOwner;
            
            if (isOnServer)
                // UNSURE ABOUT THIS MAYBE THIS IS NEEDED
                GetComponent<CharacterController>().enabled = false;
            
            }
        }


        public void OnDestroy()
        {
            if (Level.instantiateType == Level.InstantiateType.Play)
                RequestRemoval();
        }

        public virtual void SetupLocalMarshalls()
        {
            base.SetupLocalMarshalls();
        }

        public override void RequestCreation()
        {
            var p = GenerateCreationRequest(observed);

            Server.instance.WriteAll(p);
        }

        [PacketBinding.Binding]
        public override void ProcessAction(Packet p)
        {
            if (isOnServer) Server.instance.WriteAll(p, observed.localId);

            base.ProcessAction(p);
        }

        public override void RequestRemoval()
        {
            var p = GenerateRemovalRequest(observed, this);
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
                Identity = playerNetworkHandler.Identity
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
                Name = p.name,
                LocalId = p.localId,
                X = position.x,
                Y = position.y,
                Z = position.z,
                Identity = playerNetworkHandler.Identity
            };
            Debug.Log("Creating player: "+playerCreate);
            var packet = new Packet
            {
                Type = typeof(PlayerCreate).FullName,
                Handler = typeof(PlayerNetworkHandler).FullName,
                Content = Any.Pack(playerCreate)
            };
            return packet;
        }


        [PacketBinding.Binding]
        public static void HandleRemovePacket(Packet value)
        {
            PlayerRemove playerRemove;
            if (value.Content.TryUnpack(out playerRemove))
            {
                var localIdToRemove = playerRemove.LocalId;
                Debug.Log("Handling Remove for " + localIdToRemove);

                MainCaller.Do(() => { PlayerManager.playerManager.Remove(localIdToRemove); });
            }
        }

        [PacketBinding.Binding]
        public static void HandleCreatePacket(Packet value)
        {
            PlayerCreate playerCreate;
            if (value.Content.TryUnpack(out playerCreate))
            {
                Debug.Log("Received playercreation: "+playerCreate);
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

        public static void OnCreationRequest(string identity, Vector3 position, Quaternion rotation, int localId,
            string name)
        {

            MainCaller.Do(() =>
            {
                PlayerManager.playerManager.Add(localId, name, true);
                var player = PlayerManager.playerManager.GetPlayer(localId);
                PlayerNetworkHandler h = player.GetComponent<PlayerNetworkHandler>();
                h.Identity = identity;
                h.enabled = true;
                h.Setup();

                if (isOnServer)
                    //this is just for now and ugly, will fix later
                    Destroy(player.GetComponent<PlayerController>());
            });
        }
    }
}