using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerNetworkHandler : EntityNetworkHandler
    {
        public static void OnCreationRequest(string identity, LevelObjectData entityType, Vector3 position, Quaternion rotation, int localId)
        {
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
