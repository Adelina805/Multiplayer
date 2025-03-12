using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PointUIManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI mouseScore; // Assign in Inspector

    // ClientRpc to update the score UI on all clients
    [ClientRpc]
    public void UpdateScoreClientRpc(int newScore)
    {
        if (mouseScore != null)
        {
            mouseScore.text = "Mouse: " + newScore;
        }
    }
}

