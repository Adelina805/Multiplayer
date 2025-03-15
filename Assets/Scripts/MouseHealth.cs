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

                // Update the score in PointUIManager if the entity dies
                if (pointUIManager != null)
                {
                    // Assuming this is a "cat" entity. Change this logic based on your game's rules.
                    pointUIManager.AddCatScoreServerRpc(1);  // Increase cat's score by 1 (or mouse depending on entity)
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

        // If you need to update score based on which entity is taking damage, do it here
        // For example, if the cat takes damage, update mouse's score.
        if (pointUIManager != null)
        {
            // Assuming the entity is a cat or mouse, update accordingly
            pointUIManager.AddMouseScoreServerRpc(1);  // Increase mouse's score by 1 (or adjust accordingly)
        }
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
