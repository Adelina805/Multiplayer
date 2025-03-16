using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MouseCollector : NetworkBehaviour
{
    // Reference to the PointUIManager to update the score
    private PointUIManager pointUIManager;

    private void Start()
    {
        // Find PointUIManager in the scene (this is done at runtime after the mouse is spawned)
        pointUIManager = FindObjectOfType<PointUIManager>();

        if (pointUIManager == null)
        {
            Debug.LogError("PointUIManager not found in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cheese"))
        {
            NetworkObject cheeseNetworkObject = other.GetComponent<NetworkObject>();

            // Ensure the cheese object is valid and spawned before sending the ServerRpc
            if (cheeseNetworkObject != null)
            {
                if (cheeseNetworkObject.IsSpawned)
                {
                    ulong cheeseId = cheeseNetworkObject.NetworkObjectId;
                    CollectCheeseServerRpc(cheeseId);
                }
                else
                {
                    Debug.LogWarning($"Cheese object {other.gameObject.name} is not spawned or is despawned.");
                }
            }
            else
            {
                Debug.LogWarning("No NetworkObject found on the cheese.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectCheeseServerRpc(ulong cheeseId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cheeseId, out NetworkObject cheeseNetworkObject))
        {
            if (cheeseNetworkObject != null && cheeseNetworkObject.IsSpawned)
            {
                // Ensure the object is only despawned once
                cheeseNetworkObject.Despawn(true);
                Debug.Log($"Cheese with ID {cheeseId} despawned.");
                NetworkAudioManager.Instance.PlaySoundLocal(AudioClipID.CollectCheese);

                // send to UI manager to update
                if (pointUIManager != null)
                {
                    pointUIManager.AddMouseScoreServerRpc(1);
                }
            }
            else
            {
                Debug.LogWarning($"Cheese object with ID {cheeseId} is not spawned.");
            }
        }
        else
        {
            Debug.LogWarning($"Cheese object with ID {cheeseId} not found in SpawnManager.");
        }
    }
}