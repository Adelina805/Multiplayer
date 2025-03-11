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
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} requesting role update: {PlayerRoleManager.SelectedRole}");
            StartCoroutine(DelayedRoleSwap(NetworkManager.Singleton.LocalClientId, PlayerRoleManager.SelectedRole));
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
        Vector3 lastPosition = Vector3.zero;
        Quaternion lastRotation = Quaternion.identity;

        if (client.PlayerObject != null)
        {
            Debug.Log($"Saving position before despawning for Client {clientId}");
            lastPosition = client.PlayerObject.transform.position;
            lastRotation = client.PlayerObject.transform.rotation;
            Debug.Log($"Saved position: {lastPosition}, Saved rotation: {lastRotation}");
            client.PlayerObject.Despawn();
            Destroy(client.PlayerObject.gameObject);
        }

        SpawnCorrectPrefab(clientId, selectedRole, lastPosition, lastRotation);
    }

    private void SpawnCorrectPrefab(ulong clientId, string selectedRole, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject prefabToSpawn = (selectedRole == "Cat") ? catPrefab : mousePrefab;
        Debug.Log($"Spawning {selectedRole} prefab for Client {clientId} at position {spawnPosition} and rotation {spawnRotation}");

        GameObject newPlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("Spawned prefab is missing a NetworkObject component!");
            Destroy(newPlayer);
            return;
        }

        if (newPlayer.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("Rigidbody velocities reset.");
        }

        networkObject.SpawnAsPlayerObject(clientId);
        networkObject.ChangeOwnership(clientId);

        // Update client's player object reference
        UpdateClientPlayerObjectServerRpc(clientId, new NetworkObjectReference(networkObject));

        // Additional logging to ensure correct positioning
        Debug.Log($"New player object position after spawn: {newPlayer.transform.position}");
        Debug.Log($"New player object rotation after spawn: {newPlayer.transform.rotation}");
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

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        }
    }
}
