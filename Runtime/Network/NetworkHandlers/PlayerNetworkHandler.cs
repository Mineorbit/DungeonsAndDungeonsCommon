using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using General;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerNetworkHandler : EntityNetworkHandler
    {
        new Player observed;

        public virtual void Awake()
        {
            base.Awake();
            observed = GetComponent<Player>();
            GetComponent<PlayerController>().enabled = !isOnServer;
            isOwner = !isOnServer && observed.localId == NetworkManager.instance.localId;
            owner = observed.localId;
        }

        public virtual void SetupLocalMarshalls()
        {
            base.SetupLocalMarshalls();
        }

        public override void RequestCreation()
        {
            Packet p = GenerateCreationRequest(observed);

            Server.instance.WriteAll(p);
        }

        public void OnDestroy()
        {
            if(enabled)
            RequestRemoval();
        }

        public override void RequestRemoval()
        {
            Packet p = GenerateRemovalRequest(observed,this);
            Server.instance.WriteAll(p);
        }

        public static Packet GenerateRemovalRequest(Player p, PlayerNetworkHandler playerNetworkHandler)
        {
            PlayerRemove playerRemove = new PlayerRemove
            {
                LocalId = p.localId
            };

            Packet packet = new Packet
            {
                Type = typeof(PlayerRemove).FullName,
                Handler = typeof(PlayerNetworkHandler).FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(playerRemove),
                Identity = playerNetworkHandler.Identity
            };
            return packet;
        }

        public static Packet GenerateCreationRequest(Player p)
        {
            Vector3 position = p.transform.position;
            Quaternion rotation = p.transform.rotation;

            PlayerNetworkHandler playerNetworkHandler = p.GetComponent<PlayerNetworkHandler>();

            PlayerCreate playerCreate = new PlayerCreate
            {
                Name = p.name,
                LocalId = p.localId,
                X = position.x,
                Y = position.y,
                Z = position.z,
                Identity = playerNetworkHandler.Identity
            };

            Packet packet = new Packet
            {
                Type = typeof(PlayerCreate).FullName,
                Handler = typeof(PlayerNetworkHandler).FullName,
                Content = Google.Protobuf.WellKnownTypes.Any.Pack(playerCreate)
            };
            return packet;
        }


        [PacketBinding.Binding]
        public static void HandleRemovePacket(Packet value)
        {
            PlayerRemove playerRemove;
            if (value.Content.TryUnpack<PlayerRemove>(out playerRemove))
            {
                int localIdToRemove = playerRemove.LocalId;
                Debug.Log("Handling Remove for "+localIdToRemove);

                MainCaller.Do(() =>
                {
                    PlayerManager.playerManager.Remove(localIdToRemove);
                });
            }
        }

        [PacketBinding.Binding]
        public static void HandleCreatePacket(Packet value)
        {
            PlayerCreate playerCreate;
            if(value.Content.TryUnpack<PlayerCreate>(out playerCreate))
            {
                Vector3 position = new Vector3(playerCreate.X,playerCreate.Y,playerCreate.Z);
                OnCreationRequest(playerCreate.Identity,position, new Quaternion(0,0,0,0), playerCreate.LocalId ,playerCreate.Name);
            }
        }


        public static void OnCreationRequest(string identity, Vector3 position, Quaternion rotation, int localId,string name)
        {
            Debug.Log("Creating player");

            MainCaller.Do(()=> {
                PlayerManager.playerManager.Add(localId, name, true);
                GameObject player = PlayerManager.playerManager.GetPlayer(localId);
                player.GetComponent<PlayerNetworkHandler>().Identity = identity;
                player.GetComponent<PlayerNetworkHandler>().enabled = true;

                if (isOnServer)
                {
                    //this is just for now and ugly, will fix later
                    Destroy(player.GetComponent<PlayerController>());
                }
            });
            
        }
    }
}
