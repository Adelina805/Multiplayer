using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class PointUIManager : NetworkBehaviour
{
    [Header("Score Text References")]
    [SerializeField] private TextMeshProUGUI catScoreText;
    [SerializeField] private TextMeshProUGUI mouseScoreText;

    [Header("Win Panel and Text")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winnerText;

    [SerializeField] private Button playAgainButton;

    // We'll track cat and mouse scores on the server
    private int catScore;
    private int mouseScore;

    public void Start() 
    {
        // Play again button
        playAgainButton.onClick.AddListener(() => OnPlayAgainButtonClicked());
    }

    // Called by the server (or via server) to update all clients' UI
    [ClientRpc]
    public void UpdateScoreClientRpc(int newCatScore, int newMouseScore)
    {
        Debug.Log($"ClientRpc: Updating score UI => Cat: {newCatScore}, Mouse: {newMouseScore}");

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
            Debug.Log("Cat Wins !!");
            ShowWinPanelClientRpc("Cat");
        }
        else if (mouseScore >= 10)
        {
            Debug.Log("Mouse Wins !!");
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
        // Subscribe to OnLoadComplete so we know when all clients are done loading
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnAllClientsLoaded;

        // Use Netcode’s SceneManager to load "RoleSelection" for every connected client
        NetworkManager.Singleton.SceneManager.LoadScene("RoleSelection", LoadSceneMode.Single);
    }

    // This method is called once all clients finish loading the new scene
    private void OnAllClientsLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // Only act if it's the "RoleSelection" scene
        if (sceneName == "RoleSelection")
        {
            // Unsubscribe so we only do this once
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnAllClientsLoaded;

            // Shut down the Netcode session
            NetworkManager.Singleton.Shutdown();
        }
    }

    // Methods that other scripts can call to increment cat or mouse
    [ServerRpc(RequireOwnership = false)]
    public void AddCatScoreServerRpc(int increment)
    {
        catScore += increment;
        Debug.Log($"AddCatScoreServerRpc called: catScore is now {catScore}");
        UpdateScoreClientRpc(catScore, mouseScore);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMouseScoreServerRpc(int increment)
    {
        mouseScore += increment;
        Debug.Log($"AddMouseScoreServerRpc called: mouseScore is now {mouseScore}");
        UpdateScoreClientRpc(catScore, mouseScore);
    }

    // Gets current score for cat
    public int GetCatScore()
    {
        return catScore;
    }

    // Gets current score for mouse
    public int GetMouseScore()
    {
        return mouseScore;
    }
}
