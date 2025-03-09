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

// using UnityEngine;
// using UnityEngine.SceneManagement;
// using Unity.Netcode;
// using UnityEngine.UI;

// public class UIManager : MonoBehaviour
// {
//     [SerializeField] private Button startServerButton;
//     [SerializeField] private Button startHostButton;
//     [SerializeField] private Button startClientButton;

//     void Start()
//     {
//         startServerButton.onClick.AddListener(() => LoadGameSceneAndStart("server"));
//         startHostButton.onClick.AddListener(() => LoadGameSceneAndStart("host"));
//         startClientButton.onClick.AddListener(() => LoadGameSceneAndStart("client"));
//     }

//     private void LoadGameSceneAndStart(string mode)
//     {
//         Debug.Log($"Loading GameScene and starting {mode}...");
        
//         // Load the scene asynchronously and wait for completion
//         SceneManager.LoadSceneAsync("GameScene").completed += (operation) =>
//         {
//             Debug.Log("GameScene Loaded. Now starting network...");
            
//             if (NetworkManager.Singleton == null)
//             {
//                 Debug.LogError("NetworkManager is missing in GameScene!");
//                 return;
//             }

//             // Start networking AFTER the scene is fully loaded
//             switch (mode)
//             {
//                 case "server":
//                     NetworkManager.Singleton.StartServer();
//                     break;
//                 case "host":
//                     NetworkManager.Singleton.StartHost();
//                     break;
//                 case "client":
//                     NetworkManager.Singleton.StartClient();
//                     break;
//             }
//         };
//     }
// }