using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
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
        response.CreatePlayerObject = false; // We will manually spawn the player
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
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject prefabToSpawn = (PlayerRoleManager.SelectedRole == "Cat") ? catPrefab : mousePrefab;
            GameObject newPlayer = Instantiate(prefabToSpawn, GetSpawnPosition(), Quaternion.identity);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5)); // Adjust as needed
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        }
    }
}
