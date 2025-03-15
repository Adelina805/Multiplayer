// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.Netcode; // Import Netcode for Networking

// public class MouseHealth : NetworkBehaviour
// {
//     [SerializeField] private float StartingHealth;
//     private float health;

//     private NetworkVariable<int> score = new NetworkVariable<int>(0);
//     private PointUIManager uiManager; // Reference to the UI Manager

//     private void Start()
//     {
//         Health = StartingHealth;

//         // Find the UI Manager in the scene
//         uiManager = FindObjectOfType<PointUIManager>();

//         // Handle the change in score value (sync across clients)
//         score.OnValueChanged += (oldValue, newValue) =>
//         {
//             if (IsOwner) 
//             {
//                 // Pass two arguments: cat=newValue, mouse=0
//                 uiManager.UpdateScoreClientRpc(newValue, 0);
//             }
//         };

//         // On start, if we're the owner, update UI for our current score
//         if (IsOwner)
//         {
//             // Again, cat=score.Value, mouse=0
//             uiManager.UpdateScoreClientRpc(score.Value, 0);
//         }
//     }

//     public float Health
//     {
//         get
//         {
//             return health;
//         }
//         set
//         {
//             if (value < health) // Ensure we only trigger when taking damage
//             {
//                 OnDamageTakenServerRpc();
//             }

//             health = value;
//             Debug.Log(health);

//             if (health <= 0f)
//             {
//                 Destroy(gameObject);
//             }
//         }
//     }

//     [ServerRpc(RequireOwnership = false)]
//     private void OnDamageTakenServerRpc()
//     {
//         // if (gameObject.CompareTag("Mouse")) // Ensure only the mouse losing health counts
//         // {
//             // if (IsServer) // Ensure only the server updates the score
//             // {
//                 //FindObjectOfType<PointUIManager>().AddCatScoreServerRpc(1);
            
//         // Increment score on the server
//         score.Value++;

//         // Update all clients: cat=score.Value, mouse=0
//         uiManager.UpdateScoreClientRpc(score.Value, 0);
//     }
// }