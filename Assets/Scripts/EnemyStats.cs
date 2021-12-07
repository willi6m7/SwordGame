using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : NetworkBehaviour
{
    // Start is called before the first frame update
    public int health = 100;
    public int maxHealth = 100;
    public bool isBlocking;
    void Start()
    {
        health = maxHealth;
        isBlocking = false;
    }

    void Update()
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!isBlocking)
        {
            health -= damage;
            isBlocking = true;
            Debug.Log("Blocking!");
        }
        else if (isBlocking)
        {
            health -= 0;
            isBlocking = false;
            Debug.Log("Not blocking!");
        }
        if (health <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Debug.Log("Dead!");
        //implement death mechanic
        //death animation?
        //for now, just destroy the model.
        Destroy(gameObject);
    }
}
