using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class RoleSelectionUI : MonoBehaviour
{
    [SerializeField] private Button catButton;
    [SerializeField] private Button mouseButton;

    private void Start()
    {
        catButton.onClick.AddListener(() => SelectRole("Cat"));
        mouseButton.onClick.AddListener(() => SelectRole("Mouse"));
    }

    private void SelectRole(string role)
    {
        Debug.Log($"Player selected role: {role}");
        PlayerRoleManager.SelectedRole = role; // Store the chosen role

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

// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using Unity.Netcode;

// public class RoleSelectionUI : MonoBehaviour
// {
//     [SerializeField] private Button catButton;
//     [SerializeField] private Button mouseButton;

//     private void Start()
//     {
//         catButton.onClick.AddListener(() => SelectRole("Cat"));
//         mouseButton.onClick.AddListener(() => SelectRole("Mouse"));
//     }

//     private void SelectRole(string role)
//     {
//         Debug.Log($"Player selected role: {role}");
//         PlayerRoleManager.SelectedRole = role; // Store the chosen role

//         // Load GameScene and start the network
//         SceneManager.LoadSceneAsync("GameScene").completed += (operation) =>
//         {
//             Debug.Log("GameScene Loaded. Now starting network...");
            
//             if (NetworkManager.Singleton == null)
//             {
//                 Debug.LogError("NetworkManager is missing in GameScene!");
//                 return;
//             }

//             // Start networking using the stored mode from UIManager
//             switch (UIManager.SelectedMode)
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
