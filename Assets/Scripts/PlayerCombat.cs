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
    public float nextKick;
    public float blockTimer;
    public bool isBlockingCombat;

    public Animator animator;

    public AudioSource meleeHit;
    public AudioSource meleeHitSoft;
    public AudioSource meleeKick;

    //This variable is called by the PlayerStats class to resolve damage values.

    public CharacterController mpCharController;

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            //Sets the cooldown of each attack
            if (Time.time >= nextAttack)
            {
                if (Input.GetMouseButtonDown(0) && !isBlockingCombat)
                {
                    //animator.SetBool("isAttackingAnim", false);
                    MeleeAttack();
                    nextAttack = Time.time + 1f / attackSpeed;
                }
            }

            //Blocking if-statement. Dictates cooldown timer as well as what key to press. Calls up to PlayerStats to handle network interactions.
            if (Input.GetMouseButtonDown(1))
            {
                StartCoroutine(WaiterBlockAnim());
                GetComponent<PlayerStats>().PlayerBlockServerRpc();
                
            }

            //Kicking if-statement. Dictates cooldown timer as well as what key to press.
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Kick!");
                Kick();
                nextKick = Time.time + 1f / attackSpeed;
            }
        }
    }

    //Kick method. Used to break blocks.
    //Calls the PlayerKickServerRpc method in PlayerStats. Called by the Kicking if-statement in Update.
    void Kick()
    {
        meleeKick.Play();
        StartCoroutine(WaiterKickAnim());
        Collider[] kickHitPlayers = Physics.OverlapSphere(meleeAttackArea.position, attackRange, playerLayers);
        foreach (Collider player in kickHitPlayers)
        {
            if (IsOwner)
            {
                
                player.GetComponent<PlayerStats>().PlayerKickServerRpc();
            }
        }
    }

    //Script for attacking
    //Detects all "enemy"-tagged members and adds them to an array, and loops a foreach through it to deal damage via the TakeDamage method.
    //Additionally calls the Waiter coroutine that stops the player from moving while attacking.
    //Change the attack damage with the attackDamage variable.
    public void MeleeAttack()
    {
        if (IsOwner)
        {
            meleeHitSoft.volume = 0.5f;
            meleeHitSoft.Play();
            StartCoroutine(WaiterAttackAnim());
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

    //Handles the attack animation.
    IEnumerator WaiterAttackAnim()
    {
        animator.SetBool("isAttackingAnim", true);
        yield return new WaitForSeconds(1);
        animator.SetBool("isAttackingAnim", false);
    }

    //Handles the kick animation.
    IEnumerator WaiterKickAnim()
    {
        animator.SetBool("isKickingAnim", true);
        yield return new WaitForSeconds(1);
        animator.SetBool("isKickingAnim", false);
    }

    IEnumerator WaiterBlockAnim()
    {
        isBlockingCombat = true;
        animator.SetBool("isBlockingAnim", true);
        yield return new WaitForSeconds(3);
        animator.SetBool("isBlockingAnim", false);
        isBlockingCombat = false;
    }
}
