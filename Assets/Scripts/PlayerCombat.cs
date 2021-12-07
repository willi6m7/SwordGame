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
    public float attackRange;
    public int attackDamage;
    public LayerMask enemyLayers;
    public LayerMask playerLayers;
    public float attackSpeed;
    public float nextAttack;
    public float blockTimer;
    public bool kick;

    //This variable is called by the PlayerStats class to resolve damage values.
    //Is there a better way to do this? Doing this prevents the client from blocking properly.
    //public NetworkVariableBool playerIsBlocking = new NetworkVariableBool(false);

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
                GetComponent<PlayerStats>().PlayerBlockServerRpc();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Kick!");
                Kick();
            }
        }
    }


    void Kick()
    {
        kick = true;
        Collider[] kickHitPlayers = Physics.OverlapSphere(meleeAttackArea.position, attackRange, playerLayers);
        foreach (Collider player in kickHitPlayers)
        {
            if (IsOwner)
            {
                //Checks if the player is currently blocking.
                //if (player.GetComponent<PlayerCombat>().playerIsBlocking.Value && kick)
                //{
                //    player.GetComponent<PlayerCombat>().playerIsBlocking.Value = false;
                //}
            }

        }
        kick = false;
    }

    //Script for attacking
    //Detects all "enemy"-tagged members and adds them to an array, and loops a foreach through it to deal damage via the TakeDamage method.
    //Additionally calls the Waiter coroutine that stops the player from moving while attacking.
    //Change the attack damage with the attackDamage variable.
    public void MeleeAttack()
    {
        if (IsOwner)
        {
            Debug.Log("Clicked Attack!");
            Collider[] meleeHitEnemies = Physics.OverlapSphere(meleeAttackArea.position, attackRange, enemyLayers);

            //cycle through enemy NPC hits
            foreach (Collider enemy in meleeHitEnemies)
            {
                enemy.GetComponent<EnemyStats>().TakeDamageServerRpc(attackDamage);
                Debug.Log("Hit enemy: " + enemy.name);
            }

            //cycle through player hits
            //Uses a Collider[] array to find all the hit targets and then uses a foreach loop to deal out damage.
            Collider[] meleeHitPlayers = Physics.OverlapSphere(meleeAttackArea.position, attackRange, playerLayers);
            foreach (Collider player in meleeHitPlayers)
            {
                if (IsOwner)
                {
                    player.GetComponent<PlayerStats>().PlayerTakeDamageServerRpc(attackDamage);
                }
                
            }
        }

    }

    //Debugging tool for seeing the size and location of the attack
    private void OnDrawGizmosSelected()
    {
        if (meleeAttackArea == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(meleeAttackArea.position, attackRange);
    }
}
