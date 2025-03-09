using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPrefabManager : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private GameObject mousePrefab;

    private void Start() // test
    {
        // Make sure the network manager is ready and network started
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;

        // Optionally, you can also check for client-side events
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void HandleServerStarted()
    {
        Debug.Log("Server started. Spawning player...");

        // Spawn the correct player prefab based on the selected role
        if (PlayerRoleManager.SelectedRole == "Cat")
        {
            SpawnPlayerPrefab(catPrefab);
        }
        else if (PlayerRoleManager.SelectedRole == "Mouse")
        {
            SpawnPlayerPrefab(mousePrefab);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Ensure we spawn the correct player on the client as well
        if (PlayerRoleManager.SelectedRole == "Cat")
        {
            SpawnPlayerPrefab(catPrefab);
        }
        else if (PlayerRoleManager.SelectedRole == "Mouse")
        {
            SpawnPlayerPrefab(mousePrefab);
        }
    }

    private void SpawnPlayerPrefab(GameObject prefab)
    {
        // Manually spawn the player prefab in the network
        var playerObject = Instantiate(prefab);
        playerObject.GetComponent<NetworkObject>().Spawn();
    }
}
