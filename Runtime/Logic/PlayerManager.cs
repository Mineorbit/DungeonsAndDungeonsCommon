using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{ 

public class PlayerManager : MonoBehaviour
{
    public static bool acceptInput = true;
    public static PlayerManager playerManager;
    public PlayerController[] playerControllers;
    public Player[] players;

    public static int currentPlayerLocalId;

    public static Player currentPlayer;

    public void Start()
    {
        if(playerManager!=null) Destroy(this);
        playerManager= this;
        playerControllers = new PlayerController[4];
        players = new Player[4];
    }

   
    public void Reset()
    {
        for(int i = 0;i<4;i++)
        {
           Remove(i);
        }
    }
    
    public GameObject GetPlayer(int id)
    {
            return players[id].gameObject;
    }

    public void StartRound()
    {
            foreach(Player p in players)
            {
                p.OnStartRound();
            }
    }


    public void SetCurrentPlayer(int localId)
    {
        if (localId > 3 || localId < 0) return;
        if(playerControllers[localId]==null)
            {
                if (playerControllers[localId] != null)
                    playerControllers[localId] = GameObject.Find("Player"+localId).GetComponent<PlayerController>();

                if (players[localId] != null)
                    players[localId] = GameObject.Find("Player" + localId).GetComponent<Player>();
        }
        for(int i = 0; i<4;i++)
        {
                if(playerControllers[i] != null)
                    playerControllers[i].activated = localId == i;
        }
            currentPlayerLocalId = localId;
            currentPlayer = players[currentPlayerLocalId];
    }




    public Vector3 GetSpawnLocation(int i)
    {
            Vector3 location = new Vector3(i * 8, 6, 0);
            if (LevelManager.currentLevel != null)
            {
                if (LevelManager.currentLevel.spawn[i] != null)
                {
                    location = LevelManager.currentLevel.spawn[i].transform.position + new Vector3(0, 0.25f, 0);
                }
            }
            return location;
    }



    public void Remove(int localId)
    {
        if (localId > 3 || localId < 0) return;
        if(players[localId]!=null)
        if (players[localId].gameObject!=null)

        Destroy(players[localId].gameObject);
    }


    public void Add(int freeLocalId, string name, bool local)
    {
        Vector3 position = GetSpawnLocation(freeLocalId);

        LevelObjectData playerLevelObjectData = Resources.Load("LevelObjectData/Entity/Player") as LevelObjectData;
        LevelObjectData loadTargetData = Resources.Load("LevelObjectData/LevelLoadTarget") as LevelObjectData;

        GameObject playerGameObject = playerLevelObjectData.Create(position,new Quaternion(0,0,0,0),transform);
        GameObject loadTargetGameObject = loadTargetData.Create(position,new Quaternion(0,0,0,0),null);

        LevelLoadTarget loadTarget = loadTargetGameObject.GetComponent<LevelLoadTarget>();
        Player player = playerGameObject.GetComponent<Player>();
        loadTarget.target = player.transform;
        player.loadTarget = loadTarget;
        player.enabled = true;
        PlayerController playerController = playerGameObject.GetComponent<PlayerController>();

        playerController.locallyControllable = local;

        player.name = name;
        player.localId = freeLocalId;
        players[freeLocalId] = player;
        playerControllers[freeLocalId] = playerController;

    }

    public void DespawnPlayer(int localId)
    {
        if (localId > 3 || localId < 0) return;
            if (players[localId] != null)
                players[localId].Despawn();

    }

    public void SpawnPlayer(int localId,Vector3 location)
    {
        if (localId > 3 || localId < 0) return;

        if (players[localId] == null) return;

        Debug.Log("Spawning "+localId+" at "+location);
            MainCaller.Do( () => {
                players[localId].Spawn(location, new Quaternion(0,0,0,0),true);
            });
        //Move to other class Player eventually
        //Noch HUD Aktivieren
    }

    public static int GetPlayerId(GameObject player)
        {
            if (player.tag == "Player")
            {
                Player p = player.GetComponent<Player>();
                for(int i = 0; i<4;i++)
                {
                    if(p == playerManager.players[i])
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
}
}