using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabManager : NetworkBehaviour
{
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private GameObject mousePrefab;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
        }
    }

    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true; // Allow automatic spawning with default prefab
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            RequestRoleUpdateServerRpc(PlayerRoleManager.SelectedRole);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRoleUpdateServerRpc(string selectedRole, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn();
                Destroy(client.PlayerObject.gameObject);
            }
        }

        SpawnCorrectPrefab(clientId, selectedRole);
    }

    private void SpawnCorrectPrefab(ulong clientId, string selectedRole)
    {
        GameObject prefabToSpawn = (selectedRole == "Cat") ? catPrefab : mousePrefab;
        GameObject newPlayer = Instantiate(prefabToSpawn, GetSpawnPosition(), Quaternion.identity);
        newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        }
    }
}
