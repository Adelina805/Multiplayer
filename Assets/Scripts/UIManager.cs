using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    void Start()
    {
        startServerButton.onClick.AddListener(() => LoadGameSceneAndStart("server"));
        startHostButton.onClick.AddListener(() => LoadGameSceneAndStart("host"));
        startClientButton.onClick.AddListener(() => LoadGameSceneAndStart("client"));
    }

    private void LoadGameSceneAndStart(string mode)
    {
        Debug.Log($"Loading GameScene and starting {mode}...");
        
        // Load the scene asynchronously and wait for completion
        SceneManager.LoadSceneAsync("GameScene").completed += (operation) =>
        {
            Debug.Log("GameScene Loaded. Now starting network...");
            
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager is missing in GameScene!");
                return;
            }

            // Start networking AFTER the scene is fully loaded
            switch (mode)
            {
                case "server":
                    NetworkManager.Singleton.StartServer();
                    break;
                case "host":
                    NetworkManager.Singleton.StartHost();
                    break;
                case "client":
                    NetworkManager.Singleton.StartClient();
                    break;
            }
        };
    }
}