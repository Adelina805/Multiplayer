using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class RoleSelectionUI : MonoBehaviour
{
    [SerializeField] private Button catButton;
    [SerializeField] private Button mouseButton;

    public static event Action<string> OnRoleSelected; // Event for role selection

    private void Start()
    {
        catButton.onClick.AddListener(() => SelectRole("Cat"));
        mouseButton.onClick.AddListener(() => SelectRole("Mouse"));
    }
    
    private void SelectRole(string role)
    {
        Debug.Log($"Player selected role: {role}");
        PlayerRoleManager.SelectedRole = role; // Store the chosen role

        // Broadcast the role selection event
        OnRoleSelected?.Invoke(role);

        // Load GameScene and start the network
        SceneManager.LoadSceneAsync("GameScene").completed += (operation) =>
        {
            Debug.Log("GameScene Loaded. Now starting network...");
            
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager is missing in GameScene!");
                return;
            }

            // Start networking using the stored mode from UIManager
            switch (UIManager.SelectedMode)
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