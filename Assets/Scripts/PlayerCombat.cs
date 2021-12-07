using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    public Transform meleeAttackArea;
    public float attackRange = 0.4f;
    public int attackDamage = 50;
    public LayerMask enemyLayers;
    public LayerMask playerLayers;
    public float attackSpeed = 2f;
    public float nextAttack = 0f;

    //public ulong attackerId;

    //private NetworkVariableFloat health = new NetworkVariableFloat(100f);
    //private float maxHp = 100f;
    public bool playerIsBlocking = false;

    public CharacterController mpCharController;

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            //Sets the cooldown of each attack
            if (Time.time >= nextAttack)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    MeleeAttack();
                    nextAttack = Time.time + 1f / attackSpeed;
                }
            }

            //Blocking if/else statement
            if (Input.GetMouseButtonDown(1))
            {
                playerIsBlocking = true;
                Debug.Log("Player is blocking!");
            }
            else if (Input.GetMouseButtonUp(1))
            {
                playerIsBlocking = false;
                Debug.Log("Player is NOT blocking!");
            }
        }
    }

    //Script for attacking
    //Detects all "enemy"-tagged members and adds them to an array, and loops a foreach through it to deal damage via the TakeDamage method.
    //Additionally calls the Waiter coroutine that stops the player from moving while attacking.
    //Change the attack damage with the attackDamage variable
    
    private void MeleeAttack()
    {
        if (IsOwner)
        {
            //attackerId = serverRpcParams.Receive.SenderClientId;
            Debug.Log("Clicked Attack!");
            Collider[] meleeHitEnemies = Physics.OverlapSphere(meleeAttackArea.position, attackRange, enemyLayers);

            //cycle through enemy hits
            foreach (Collider enemy in meleeHitEnemies)
            {
                enemy.GetComponent<EnemyStats>().TakeDamageServerRpc(attackDamage);
                Debug.Log("Hit enemy: " + enemy.name);
            }

            //cycle through player hits
            Collider[] meleeHitPlayers = Physics.OverlapSphere(meleeAttackArea.position, attackRange, playerLayers);
            List<Collider> alreadyMeleedPlayers = new List<Collider>();
            foreach (Collider player in meleeHitPlayers)
            {
                Debug.Log("1");
                if (IsOwner)
                {
                    Debug.Log("2");
                    if (!alreadyMeleedPlayers.Contains(player))
                    {
                        Debug.Log(player.name);
                        Debug.Log(GetComponent<PlayerStats>().health.Value);
                        player.GetComponent<PlayerStats>().PlayerTakeDamageServerRpc(attackDamage);
                        alreadyMeleedPlayers.Add(player);
                        
                    }
                    else Debug.Log("Already hit!");
                }
                
            }
            //StartCoroutine(Waiter());
        }

    }
    /*
    //Method handling player damage
    [ServerRpc]
    void PlayerTakeDamageServerRpc(int dmg)
    {
        if (!playerIsBlocking)
        {
            health.Value -= dmg;
            Debug.Log("Struck!");
        }
        else if (playerIsBlocking)
        {
            health.Value -= 0f;
            playerIsBlocking = false;
            Debug.Log("Player Not blocking!");
        }
        if (health.Value <= 0f)
        {
            Death();
        }
    }
    */




    //Debugging tool for seeing the size and location of the attack
    private void OnDrawGizmosSelected()
    {
        if (meleeAttackArea == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(meleeAttackArea.position, attackRange);
    }

    //Waiter coroutine
    /*
    IEnumerator Waiter()
    {
        mpCharController.enabled = false;
        yield return new WaitForSeconds(1.5f);
        mpCharController.enabled = true;
    }*/
}
