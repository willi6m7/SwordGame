using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPPlayerStats : NetworkBehaviour
{
    public NetworkVariableFloat health = new NetworkVariableFloat(200f);
    private float maxHp = 200f;
    public NetworkVariableBool playerBlocking = new NetworkVariableBool(false);
    //public NetworkVariableBool kick = new NetworkVariableBool(false);
    public float blockTimer = 3f;
    private float kickRecovery = 5f;
    private bool isRecoveringFromKick;

    public ulong spawnPlayerId;

    public NetworkVariableInt kills = new NetworkVariableInt(0);

    public AudioSource deathThwack;

    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        
    }

    //ServerRpc for blocking. Makes the blocking interactable between players.
    //Starts the WaiterBlocking coroutine that has most of the logic.
    [ServerRpc(RequireOwnership = false)]
    public void PlayerBlockServerRpc()
    {
        
        StartCoroutine(WaiterBlocking());
        
    }

    //ServerRpc for kicking. Makes the kicking interactable between players.
    //Starts the WaiterKickDisabler coroutine that has most of the logic.
    [ServerRpc(RequireOwnership = false)]
    public void PlayerKickServerRpc()
    {
        StartCoroutine(WaiterKickDisabler());
    }

    //ServerRpc for taking damage. If the attack is blocked, negate the damage.
    [ServerRpc(RequireOwnership = false)]
    public void PlayerTakeDamageServerRpc(float damage, ServerRpcParams serverRpcParams = default)
    {
        spawnPlayerId = serverRpcParams.Receive.SenderClientId;
        if (playerBlocking.Value == false)
        {
            health.Value -= damage;
            if (health.Value <= 0)
            {
                IncreaseKillCountServerRpc(GetComponent<MPPlayerStats>().spawnPlayerId);
                Death();
            }
        }
    }

    //Death mechanic.
    private void Death()
    {
        Debug.Log("Player is dead!");
        deathThwack.Play();
        RespawnPlayerServerRpc();
        ResetPlayerClientRpc();
    }

    //ClientRpc for resetting the player's position. Pulled from in-class assignments.
    [ClientRpc]
    private void ResetPlayerClientRpc()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        //GameObject[] spawnPointsTwo = GameObject.FindGameObjectsWithTag("SpawnPoint2");

        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int index = UnityEngine.Random.Range(0, spawnPoints.Length);
        //int indexTwo = UnityEngine.Random.Range(0, spawnPoints.Length);
        GameObject currentPoint = spawnPoints[index];
        //GameObject currentPointTwo = spawnPointsTwo[indexTwo];

        GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPoints[index].transform.position;
        GetComponent<CharacterController>().enabled = true;

        //Spawn player 2 mechanic
        //Does not work
        /*
        GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPointsTwo[indexTwo].transform.position;
        GetComponent<CharacterController>().enabled = true;
        */
    }
    
    //ServerRpc for resetting the player's stats. Pulled from in-class assignments and altered to reset some stats applicable here.
    [ServerRpc(RequireOwnership = false)]
    private void RespawnPlayerServerRpc()
    {
        health.Value = maxHp;
        playerBlocking.Value = false;
        isRecoveringFromKick = false;

        animator.SetBool("isBlockingAnim", false);
        animator.SetBool("isAttackingAnim", false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseKillCountServerRpc(ulong spawnPlayerId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObj in players)
        {
            if (playerObj.GetComponent<NetworkObject>().OwnerClientId == spawnPlayerId)
            {
                playerObj.GetComponent<MPPlayerStats>().kills.Value++;
                if (kills.Value >= 10)
                {
                    Debug.Log("guh");
                }
            }
        }
    }

    IEnumerator WaiterBlocking()
    {
        if (!playerBlocking.Value && !isRecoveringFromKick)
        {
            playerBlocking.Value = true;
            animator.SetBool("isBlockingAnim", true);
            Debug.Log("Blocking!");
            yield return new WaitForSeconds(blockTimer);
            playerBlocking.Value = false;
            animator.SetBool("isBlockingAnim", false);
            Debug.Log("Player is not blocking!");
        }
        else Debug.Log("Already blocking!");
    }

    IEnumerator WaiterKickDisabler()
    {
        if (playerBlocking.Value)
        {
            playerBlocking.Value = false;
            isRecoveringFromKick = true;
            animator.SetBool("isBlockingAnim", false);
            yield return new WaitForSeconds(kickRecovery);
            isRecoveringFromKick = false;
        }
    }
}
