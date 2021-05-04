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
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            PlayerCreate playerCreate = new PlayerCreate
            {
                Name = observed.name,
                LocalId = observed.localId,
                X = position.x,
                Y = position.y,
                Z = position.z,
                Identity = this.Identity
            };

            Server.instance.WriteAll(playerCreate);
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
            PlayerManager.playerManager.Add(localId,"Test",local);
            PlayerManager.playerManager.SpawnPlayer(localId, position);

            GameObject player = PlayerManager.playerManager.GetPlayer(localId);
            player.GetComponent<PlayerNetworkHandler>().Identity = identity;

            if(local)
            {
                PlayerManager.playerManager.SetCurrentPlayer(localId);
            }
        }
    }
}
