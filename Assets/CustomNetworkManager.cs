// using Unity.Netcode;
// using UnityEngine;

// public class CustomNetworkManager : NetworkBehaviour
// {
//     [SerializeField] private GameObject catPrefab;
//     [SerializeField] private GameObject mousePrefab;

//     public override void CustomOnNetworkSpawn()
//     {
//         if (IsServer)
//         {
//             NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
//         }
//     }

//     private void CustomSpawnPlayer(ulong clientId)
//     {
//         if (!IsServer) return; // Only the server should handle player spawning

//         Debug.Log($"Spawning player for Client {clientId}");

//         GameObject prefabToSpawn = ChoosePlayerPrefab(clientId);
//         if (prefabToSpawn == null)
//         {
//             Debug.LogError("Prefab selection failed! No prefab assigned.");
//             return;
//         }

//         GameObject playerInstance = Instantiate(prefabToSpawn);
//         NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

//         if (networkObject != null)
//         {
//             networkObject.SpawnAsPlayerObject(clientId);
//             Debug.Log($"Spawned {prefabToSpawn.name} for Client {clientId}");
//         }
//         else
//         {
//             Debug.LogError("NetworkObject component missing on spawned player prefab.");
//         }
//     }

//     private GameObject CustomChoosePlayerPrefab(ulong clientId)
//     {
//         // Example: Assigning based on even/odd client IDs (You should replace this with actual role selection logic)
//         return clientId % 2 == 0 ? catPrefab : mousePrefab;
//     }

//     private void CustomOnDestroy()
//     {
//         if (IsServer && NetworkManager.Singleton != null)
//         {
//             NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
//         }
//     }
// }
