using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CheeseSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject cheesePrefab; // Assign Cheese prefab in Inspector
    [SerializeField] private int cheeseCount = 20; // Number of cheese objects to spawn
    [SerializeField] private Vector3 spawnArea = new Vector3(30f, 1f, 30f); // Define spawn area size

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server should spawn objects
        {
            SpawnCheeseObjects();
        }
    }

    private void SpawnCheeseObjects()
    {
        for (int i = 0; i < cheeseCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                spawnArea.y, // Keep cheese above ground
                Random.Range(-spawnArea.z, spawnArea.z)
            );

            GameObject cheeseInstance = Instantiate(cheesePrefab, randomPosition, Quaternion.identity);
            NetworkObject networkObj = cheeseInstance.GetComponent<NetworkObject>();

            if (networkObj != null)
            {
                networkObj.Spawn(); // Only the server should spawn this
                Debug.Log($"Cheese spawned with ID: {networkObj.NetworkObjectId}");
            }
            else
            {
                Debug.Log("Cheese prefab does not have a NetworkObject component!");
            }
        }
    }

    // private void SpawnCheeseObjects()
    // {
    //     for (int i = 0; i < cheeseCount; i++)
    //     {
    //         Vector3 randomPosition = new Vector3(
    //             Random.Range(-spawnArea.x, spawnArea.x),
    //             spawnArea.y, // Keep cheese above ground
    //             Random.Range(-spawnArea.z, spawnArea.z)
    //         );

    //         GameObject cheeseInstance = Instantiate(cheesePrefab, randomPosition, Quaternion.identity);
    //         cheeseInstance.GetComponent<NetworkObject>().Spawn(); // Spawn on the network
    //         Debug.Log($"Cheese spawned with ID: {cheeseInstance.GetComponent<NetworkObject>().NetworkObjectId}");
    //     }
    // }
}

