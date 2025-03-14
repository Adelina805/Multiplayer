using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PointUIManager : NetworkBehaviour
{
    [Header("Score Text References")]
    [SerializeField] private TextMeshProUGUI catScoreText;
    [SerializeField] private TextMeshProUGUI mouseScoreText;

    [Header("Win Panel and Text")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winnerText;

    // We'll track cat and mouse scores on the server
    private int catScore;
    private int mouseScore;

    // Called by the server (or via server) to update all clients' UI
    [ClientRpc]
    public void UpdateScoreClientRpc(int newCatScore, int newMouseScore)
    {
        // Store the updated scores locally on each client
        catScore = newCatScore;
        mouseScore = newMouseScore;

        // Update Cat UI
        if (catScoreText != null)
        {
            catScoreText.text = "Cat: " + catScore;
        }

        // Update Mouse UI
        if (mouseScoreText != null)
        {
            mouseScoreText.text = "Mouse: " + mouseScore;
        }

        // Check for winner
        if (catScore >= 10)
        {
            ShowWinPanelClientRpc("Cat");
        }
        else if (mouseScore >= 10)
        {
            ShowWinPanelClientRpc("Mouse");
        }
    }

    // Show the "XXX wins !!" panel on all clients
    [ClientRpc]
    private void ShowWinPanelClientRpc(string winnerName)
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (winnerText != null)
            winnerText.text = winnerName + " wins !!";
    }

    // This is called by the "Play Again" button in the Win Panel
    public void OnPlayAgainButtonClicked()
    {
        // Let the server handle the reset
        Debug.Log("Play Again button clicked on client side");
        RequestRestartServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestRestartServerRpc()
    {
        Debug.Log("Server received Play Again request");
        
        // Reset both scores to 0 on the server
        catScore = 0;
        mouseScore = 0;

        // Update every client’s UI back to 0
        UpdateScoreClientRpc(catScore, mouseScore);

        // Hide the win panel on all clients
        HideWinPanelClientRpc();
    }

    [ClientRpc]
    private void HideWinPanelClientRpc()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    // Methods that other scripts can call to increment cat or mouse
    [ServerRpc(RequireOwnership = false)]
    public void AddCatScoreServerRpc(int increment)
    {
        catScore += increment;
        UpdateScoreClientRpc(catScore, mouseScore);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMouseScoreServerRpc(int increment)
    {
        mouseScore += increment;
        UpdateScoreClientRpc(catScore, mouseScore);
    }
}
