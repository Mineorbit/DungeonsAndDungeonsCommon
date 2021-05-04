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

        public static void HandleCreatePacket(Packet p)
        {
            PlayerCreate playerCreate;
            if(p.Content.TryUnpack<PlayerCreate>(out playerCreate))
            {
                Vector3 position = new Vector3(playerCreate.X,playerCreate.Y,playerCreate.Z);
                OnCreationRequest(playerCreate.Identity,position, new Quaternion(0,0,0,0), playerCreate.LocalId ,playerCreate.Name);
            }
        }


        public static void OnCreationRequest(string identity, Vector3 position, Quaternion rotation, int localId,string name)
        {
            Debug.Log("Creating player");

            bool local = localId == NetworkManager.instance.localId;

            MainCaller.Do(()=> {
                PlayerManager.playerManager.Add(localId, name, local);

                GameObject player = PlayerManager.playerManager.GetPlayer(localId);
                player.GetComponent<PlayerNetworkHandler>().Identity = identity;

                if (local)
                {
                    PlayerManager.playerManager.SetCurrentPlayer(localId);
                }

            });
            
        }
    }
}
