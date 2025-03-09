using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkBehaviour
{
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private GameObject mousePrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return; // Only the server should handle player spawning

        // Decide which prefab to spawn
        GameObject prefabToSpawn = ChoosePlayerPrefab(clientId);

        // Instantiate and spawn the player
        GameObject playerInstance = Instantiate(prefabToSpawn);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId);
        }
    }

    private GameObject ChoosePlayerPrefab(ulong clientId)
    {
        // Example: Assigning based on even/odd client IDs (You should replace this with actual role selection logic)
        return clientId % 2 == 0 ? catPrefab : mousePrefab;
    }

    private void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }
}
