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

        Instantiable t = Resources.Load("LevelObjectData/Entity/Player") as Instantiable;
        GameObject g = t.Create(new Vector3(freeLocalId * 8, 6, 0), transform);

        Player player = g.AddComponent<Player>();
        PlayerController playerController = g.AddComponent<PlayerController>();

        playerController.locallyControllable = local;

        player.name = name;
        player.localId = freeLocalId;

        Debug.Log("1234");
        players[freeLocalId] = player;
        playerControllers[freeLocalId] = playerController;

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