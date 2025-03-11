using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MouseCollector : NetworkBehaviour
{
    public TextMeshProUGUI scoreText; // Assign in Inspector
    private NetworkVariable<int> score = new NetworkVariable<int>(0);

    private void Start()
    {
        if (IsOwner)
        {
            UpdateScoreUI(score.Value);
        }
        
        score.OnValueChanged += (oldValue, newValue) =>
        {
            if (IsOwner)
            {
                UpdateScoreUI(newValue);
            }
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return; // Ensure only the local player can collect

        if (other.CompareTag("Cheese"))
        {
            NetworkObject cheeseNetworkObject = other.GetComponent<NetworkObject>();
            if (cheeseNetworkObject != null && cheeseNetworkObject.IsSpawned)
            {
                CollectCheese(cheeseNetworkObject);
            }
        }
    }

    [ServerRpc]
    private void CollectCheeseServerRpc()
    {
        score.Value++;
    }

    private void CollectCheese(NetworkObject cheeseNetworkObject)
    {
        cheeseNetworkObject.Despawn(true); // Despawn cheese object across network
        CollectCheeseServerRpc(); // Update score on the server
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore;
        }
    }
}
