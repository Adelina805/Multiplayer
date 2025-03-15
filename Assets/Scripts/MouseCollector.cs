using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MouseCollector : NetworkBehaviour
{
    private NetworkVariable<int> score = new NetworkVariable<int>(0);
    private PointUIManager uiManager; // Reference to the UI Manager

    private void Start()
    {
        // Find the UI Manager in the scene
        uiManager = FindObjectOfType<PointUIManager>();

        // Handle the change in score value (sync across clients)
        score.OnValueChanged += (oldValue, newValue) =>
        {
            if (IsOwner) 
            {
                // Get the current cat score from the UI manager
                int currentCatScore = uiManager.GetCatScore();
                uiManager.UpdateScoreClientRpc(currentCatScore, newValue);
            }
        };

        // On start, if we're the owner, update UI for our current score
        if (IsOwner)
        {
            // Get the current cat score from the UI manager
            int currentCatScore = uiManager.GetCatScore();
            uiManager.UpdateScoreClientRpc(currentCatScore, score.Value);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return; // Only the owner should process collisions

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

        // Increment score on the server
        score.Value++;

        // Get the current cat score from the UI manager
        int currentCatScore = uiManager.GetCatScore();
        uiManager.UpdateScoreClientRpc(currentCatScore, score.Value);
    }
}