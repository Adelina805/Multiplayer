using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabManager : NetworkBehaviour
{
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private GameObject mousePrefab;

    private void Start()
    {
        // Ensure the prefab manager handles player connection approval
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
        }
    }

    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Automatically approve the connection
        response.Approved = true;
        response.CreatePlayerObject = false; // Disable automatic player object creation
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // When the server spawns, it spawns players for all connected clients
            SpawnPlayer();
        }
        else
        {
            // For the client, trigger the spawn player request
            RequestSpawnPlayerServerRpc();
        }
    }

    private void SpawnPlayer()
    {
        if (IsServer)
        {
            Debug.Log("Spawning player on the server...");

            // Get the player prefab to spawn based on role selection
            string selectedRole = PlayerRoleManager.SelectedRole;
            GameObject prefabToSpawn = (selectedRole == "Cat") ? catPrefab : mousePrefab;

            // Spawn player for each connected client
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                GameObject newPlayer = Instantiate(prefabToSpawn, GetSpawnPosition(), Quaternion.identity);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
            }
        }
        else
        {
            Debug.Log("Not the server, cannot spawn player.");
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // Random spawn position for each player
        return new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
    }

    // ServerRpc to request the server to spawn the player for the client
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        // The server will spawn the player when the client requests it
        SpawnPlayer();
    }

    private void OnDestroy()
    {
        // Clean up by removing the connection approval callback
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        }
    }
}
