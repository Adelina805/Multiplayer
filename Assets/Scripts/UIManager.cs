using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    public static string SelectedMode; // Store the network mode globally

    void Start()
    {
        startServerButton.onClick.AddListener(() => LoadRoleSelection("server"));
        startHostButton.onClick.AddListener(() => LoadRoleSelection("host"));
        startClientButton.onClick.AddListener(() => LoadRoleSelection("client"));
    }

    private void LoadRoleSelection(string mode)
    {
        Debug.Log($"Loading RoleSelection scene before starting {mode}...");
        SelectedMode = mode; // Store the selected mode
        SceneManager.LoadScene("RoleSelection"); // Go to role selection screen
    }
}