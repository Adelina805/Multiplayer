using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHealth : MonoBehaviour
{
    [SerializeField] private float StartingHealth;
    private float health;

    // Reference to the PointUIManager to update the score
    [SerializeField] private PointUIManager pointUIManager;

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
                Destroy(gameObject);
                
                // Update the score in PointUIManager if the entity takes damage
                if (pointUIManager != null)
                {
                    pointUIManager.AddCatScoreServerRpc(1);  // Increase cat's score by 1 
                }
            }
        }
    }

    void Start()
    {
        Health = StartingHealth;
    }

    // Call this method to apply damage to the entity and update the score
    public void TakeDamage(float damage)
    {
        Health -= damage;

        // // Update the score in PointUIManager when damage is taken
        // if (health > 0f && pointUIManager != null)
        // {
        //     pointUIManager.AddCatScoreServerRpc(1);  // Increase cat's score by 1 
        // }
    }
}
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
