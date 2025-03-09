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
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (IsServer)
        {
            Debug.Log("Spawning player on the server...");
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                GameObject prefabToSpawn = (PlayerRoleManager.SelectedRole == "Cat") ? catPrefab : mousePrefab;
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

    private void OnDestroy()
    {
        // Clean up by removing the connection approval callback
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        }
    }
}
