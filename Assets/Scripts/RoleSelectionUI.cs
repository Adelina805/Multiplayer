using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class RoleSelectionUI : MonoBehaviour
{
    [SerializeField] private Button catButton;
    [SerializeField] private Button mouseButton;

    public static event Action<string> OnRoleSelected;

    private void Start()
    {
        catButton.onClick.AddListener(() => SelectRole("Cat"));
        mouseButton.onClick.AddListener(() => SelectRole("Mouse"));
    }

    private void SelectRole(string role)
    {
        Debug.Log($"Player selected role: {role}");
        PlayerRoleManager.SelectedRole = role;

        OnRoleSelected?.Invoke(role);

        SceneManager.LoadSceneAsync("GameScene").completed += (operation) =>
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager is missing in GameScene!");
                return;
            }

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
