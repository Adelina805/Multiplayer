using Unity.Netcode;
using UnityEngine;
using System.Collections;

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
        if (IsClient)
        {
            Debug.Log("Client spawned.");
            // if (IsOwner)
            // {
            //     Debug.Log("Client is owner.");
            //     if (NetworkManager.Singleton.LocalClient.PlayerObject == null)
            //     {
            //         Debug.LogError("PlayerObject is null before role update request!");
            //         return;
            //     }

                Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} requesting role update: {PlayerRoleManager.SelectedRole}");
                StartCoroutine(DelayedRoleSwap(NetworkManager.Singleton.LocalClientId, PlayerRoleManager.SelectedRole));
            // }
            // else
            // {
            //     Debug.Log("Client is not owner.");
            // }
        }
    }

    private IEnumerator DelayedRoleSwap(ulong clientId, string selectedRole)
    {
        yield return new WaitForSeconds(1.0f); // Increased delay to ensure proper initialization
        Debug.Log($"Client {clientId} sending role update request for: {selectedRole}");
        RequestRoleUpdateServerRpc(selectedRole);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRoleUpdateServerRpc(string selectedRole, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Server received role update request from Client {clientId}: {selectedRole}");

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogError($"Client {clientId} not found in ConnectedClients!");
            return;
        }

        var client = NetworkManager.Singleton.ConnectedClients[clientId];
        if (client.PlayerObject != null)
        {
            Debug.Log($"Despawning default player object for Client {clientId}");
            client.PlayerObject.Despawn();
            Destroy(client.PlayerObject.gameObject);
        }

        SpawnCorrectPrefab(clientId, selectedRole);
    }

    private void SpawnCorrectPrefab(ulong clientId, string selectedRole)
    {
        GameObject prefabToSpawn = (selectedRole == "Cat") ? catPrefab : mousePrefab;
        Debug.Log($"Spawning {selectedRole} prefab for Client {clientId}");

        GameObject newPlayer = Instantiate(prefabToSpawn, GetSpawnPosition(), Quaternion.identity);
        NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("Spawned prefab is missing a NetworkObject component!");
            Destroy(newPlayer);
            return;
        }

        networkObject.SpawnAsPlayerObject(clientId);
        networkObject.ChangeOwnership(clientId);

        // Update client's player object reference
        UpdateClientPlayerObjectServerRpc(clientId, new NetworkObjectReference(networkObject));
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateClientPlayerObjectServerRpc(ulong clientId, NetworkObjectReference newPlayer, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != clientId)
        {
            return;
        }

        if (newPlayer.TryGet(out NetworkObject networkObject))
        {
            Debug.Log($"Updating Client {clientId}'s player object to {networkObject.name}");
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject = networkObject;
        }
        else
        {
            Debug.LogError($"Failed to get NetworkObject for Client {clientId}");
        }
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
