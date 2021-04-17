﻿using System.Collections;
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


    public void SetCurrentPlayer(int localId)
    {
        if (localId > 3 || localId < 0) return;
        if(playerControllers[localId]==null)
        {
            playerControllers[localId] = GameObject.Find("Player"+localId).GetComponent<PlayerController>();
            players[localId] = GameObject.Find("Player" + localId).GetComponent<Player>();
        }
        for(int i = 0; i<4;i++)
        {
            playerControllers[i].activated = localId == i;
        }
        currentPlayerLocalId = localId;
    }




    Vector3 GetSpawnLocation(int i)
    {
            Vector3 location = new Vector3(i * 8, 6, 0);
            if(LevelManager.currentLevel.spawn[i] != null)
            {
                location = LevelManager.currentLevel.spawn[i].transform.position;
            }
            return location;
    }



    public void Remove(int localId)
    {
        if (localId > 3 || localId < 0) return;
        if(playerControllers[localId]!=null)
        if (playerControllers[localId].gameObject!=null)

        Destroy(playerControllers[localId].gameObject);

        if(currentPlayerLocalId == localId)
        {
            currentPlayerLocalId = -1;
        }

    }


    public void Add(int freeLocalId, string name, bool local)
    {
        if(LevelManager.currentLevel != null)
        {
        Vector3 position = GetSpawnLocation(freeLocalId);

        LevelObjectData levelObjectData = Resources.Load("LevelObjectData/Entity/Player") as LevelObjectData;
        GameObject g = LevelManager.currentLevel.AddDynamic(levelObjectData,position,new Quaternion(0,0,0,0));
        Player player = g.GetComponent<Player>();
        PlayerController playerController = g.GetComponent<PlayerController>();

        playerController.locallyControllable = local;

        player.name = name;
        player.localId = freeLocalId;
        players[freeLocalId] = player;
        playerControllers[freeLocalId] = playerController;
        }

        }

    public void DespawnPlayer(int localId)
    {
        if (localId > 3 || localId < 0) return;
        if(playerControllers[localId]!= null)
        playerControllers[localId].gameObject.SetActive(false);

    }

    public void SpawnPlayer(int localId,Vector3 location)
    {
        if (localId > 3 || localId < 0) return;


        if (playerControllers[localId] == null) return;


        playerControllers[localId].gameObject.SetActive(true);
        //Move to other class Player eventually
        playerControllers[localId].transform.position = location;
        //Noch HUD Aktivieren
    }
}
}