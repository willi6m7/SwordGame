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
    //public NetworkVariableBool kick = new NetworkVariableBool(false);
    public float blockTimer = 10f;
    private float kickRecovery = 5f;
    private bool isRecoveringFromKick;

    public AudioSource deathThwack;

    public Animator animator;

    // Update is called once per frame
    //If the player "dies" they respawn at one of the spawn points via the RespawnPlayerServerRpc() and the ResetPlayerClientRpc() methods.
    void Update()
    {
        if (health.Value <= 0)
        {
            Death();
        }
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
    public void PlayerTakeDamageServerRpc(float damage)
    {
        if (playerBlocking.Value == false)
        {
            health.Value -= damage;
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

        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int index = UnityEngine.Random.Range(0, spawnPoints.Length);
        GameObject currentPoint = spawnPoints[index];

        GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPoints[index].transform.position;
        GetComponent<CharacterController>().enabled = true;
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
