using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MouseHealth : NetworkBehaviour
{
    [SerializeField] private float StartingHealth;
    private float health;

    // Reference to the PointUIManager to update the score
    private PointUIManager pointUIManager;

    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            Debug.Log($"Entity health: {health}");

            if (health <= 0f)
            {
                Debug.Log($"Entity Destroyed: health is {health}");
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        // Find PointUIManager in the scene (this is done at runtime after the mouse is spawned)
        pointUIManager = FindObjectOfType<PointUIManager>();

        if (pointUIManager == null)
        {
            Debug.LogError("PointUIManager not found in the scene!");
        }

        Health = StartingHealth;
    }

    // apply damage to the entity and update the score
    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage() called!");

        // Grant the cat points equal to the damage taken
        if (pointUIManager != null)
        {
            // Cast damage to int in case it's non-integer.
            pointUIManager.AddCatScoreServerRpc((int)damage);
        }

        Health -= damage;
    }
}

// refference to this starter code:

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Entity : MonoBehaviour
// {
//     [SerializeField] private float StartingHealth;
//     private float health;

//     public float Health
//     {
//         get
//         {
//             return health;
//         }
//         set
//         {
//             health = value;
//             Debug.Log($"Entity health: {health}");

//             if (health <= 0f)
//             {
//                 Destroy(gameObject);
//             }
//         }
//     }
//     void Start()
//     {
//         Health = StartingHealth;
//     }
// }
