using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabManager : NetworkBehaviour
{
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private GameObject mousePrefab;

    private void OnEnable()
    {
        RoleSelectionUI.OnRoleSelected += HandleRoleSelected;
    }

    private void OnDisable()
    {
        RoleSelectionUI.OnRoleSelected -= HandleRoleSelected;
    }

    private void HandleRoleSelected(string selectedRole)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            // Request the server to spawn the player with the selected role
            RequestSpawnPlayerServerRpc(selectedRole);
        }
    }

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
        string selectedRole = PlayerRoleManager.SelectedRole; // Get the selected role

        if (IsServer)
        {
            // When the server spawns, it spawns players for all connected clients
            SpawnPlayer(selectedRole);
        }
        else
        {
            // For the client, trigger the spawn player request and pass the selected role
            RequestSpawnPlayerServerRpc(selectedRole);
        }
    }

    private void SpawnPlayer(string selectedRole)
    {
        if (IsServer)
        {
            Debug.Log("Spawning player on the server...");

            // Get the player prefab to spawn based on role selection
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
    private void RequestSpawnPlayerServerRpc(string selectedRole, ServerRpcParams rpcParams = default)
    {
        // The server will spawn the player with the correct prefab based on the selected role
        SpawnPlayer(selectedRole);
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