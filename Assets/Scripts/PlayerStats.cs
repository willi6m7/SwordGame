using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariableFloat health = new NetworkVariableFloat(200f);
    private float maxHp = 200f;
    public NetworkVariableBool playerBlocking = new NetworkVariableBool(false);
    public float blockTimer = 10f;

    // Update is called once per frame
    //If the player "dies" they respawn at one of the spawn points via the RespawnPlayerServerRpc() and the ResetPlayerClientRpc() methods.
    void Update()
    {
        if (health.Value <= 0)
        {
            RespawnPlayerServerRpc();
            ResetPlayerClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerBlockServerRpc()
    {
        
        StartCoroutine(WaiterBlocking());
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerTakeDamageServerRpc(float damage)
    {
        if (playerBlocking.Value == false)
        {
            health.Value -= damage;
            if (health.Value <= 0)
            {
                Death();
            }
        }
        
    }

    private void Death()
    {
        Debug.Log("Player is dead!");
    }

    [ClientRpc]
    private void ResetPlayerClientRpc()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int index = UnityEngine.Random.Range(0, spawnPoints.Length);
        GameObject currentPoint = spawnPoints[index];

        GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPoints[index].transform.position;
        GetComponent<CharacterController>().enabled = true;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RespawnPlayerServerRpc()
    {
        health.Value = maxHp;
    }

    IEnumerator WaiterBlocking()
    {
        playerBlocking.Value = true;
        Debug.Log("Blocking!");
        yield return new WaitForSeconds(blockTimer);
        playerBlocking.Value = false;
        Debug.Log("Player is not blocking!");
    }
}
