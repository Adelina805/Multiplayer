using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using System;
using TMPro;

public class Player : NetworkBehaviour
{
    // Reference to the PointUIManager to update the score
    private PointUIManager pointUIManager;
    private int catScore, mouseScore;

    [SerializeField] private float speed;
    [SerializeField] private float jumpForce; // Jump power
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private string inputNameHorizontal;
    [SerializeField] private string inputNameVertical;
    [SerializeField] private Color color;
    
    [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform groundCheck; // Empty object at player's feet
    [SerializeField] private LayerMask groundLayer; // Assign ground layer in Inspector

    // --- Footsteps settings ---
    [SerializeField] private float footstepInterval = 0.5f; // Seconds between footsteps
    private float footstepTimer = 0f;

    private Rigidbody rb;
    private Renderer renderer;

    private float inputHorizontal;
    private float inputVertical;
    
    private float mouseX;
    private float mouseY;
    private float xRotation = 0f; // Tracks vertical camera rotation

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        renderer = GetComponentInChildren<Renderer>();
        renderer.material.color = color;
        rb.freezeRotation = true;

        if (IsOwner)
        {
            Debug.Log("IsOwner: " + IsOwner);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Application.targetFrameRate = 60; // Lock to 60 FPS
        Physics.gravity = new Vector3(0, -30f, 0); // Adjust for snappier jumps

        // Find PointUIManager in the scene (this is done at runtime after the mouse is spawned)
        pointUIManager = FindObjectOfType<PointUIManager>();

        if (pointUIManager == null)
        {
            Debug.LogError("PointUIManager not found in the scene!");
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            camera.Priority = 1;
        } 
        else 
        {
            camera.Priority = 0;
        }

        // get scores
        if (IsClient && !IsServer)
        {
            RequestScoreServerRpc();
        }

    }

    private void Update()
    {
        if (!IsOwner) return; // Only allow movement for the local player

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed!"); 
            ToggleCursor();
        }

        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Debug.Log("Jump!");
            NetworkAudioManager.Instance.PlaySoundGlobal(AudioClipID.Jump);
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce / Time.fixedDeltaTime, ForceMode.Impulse);
        }

        if (!IsGrounded()) 
        {
            Debug.Log("is not on da ground");
            rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration); // Faster fall
        }

        // Only play footsteps if the player is on the ground and moving
        if (IsGrounded() && (Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f))
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                // Play footsteps sound locally
                NetworkAudioManager.Instance.PlaySoundLocal(AudioClipID.Footsteps);
                footstepTimer = 0f;
            }
        }
        else
        {
            // Reset the timer so footsteps play immediately when movement resumes
            footstepTimer = footstepInterval;
        }

        // Mouse look
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player left and right (yaw)
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, mouseX, 0));

        // Rotate the camera up and down (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevents flipping

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Rotate only the camera
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 moveDirection = transform.forward * inputVertical + transform.right * inputHorizontal;

        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y, moveDirection.z * speed);
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
    }

    private void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

        private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Cheese") || collision.gameObject.CompareTag("Object"))
        {
            NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();

            if (networkObject != null && networkObject.IsSpawned)
            {
                RequestOwnershipServerRpc(networkObject);
            }
            else
            {
                Debug.LogWarning("Object is either not found or not spawned.");
            }
        }
    }

    // allow client to interact with objects and cheese
    [ServerRpc]
    private void RequestOwnershipServerRpc(NetworkObjectReference objectReference, ServerRpcParams rpcParams = default)
    {
        if (objectReference.TryGet(out NetworkObject networkObject))
        {
            networkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        }
    }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (!IsOwner) return;

    //     if (collision.gameObject.CompareTag("Cheese") )
    //     {
    //         NetworkObject cheeseNetworkObject = collision.gameObject.GetComponent<NetworkObject>();

    //         if (cheeseNetworkObject != null && cheeseNetworkObject.IsSpawned)
    //         {
    //             // allows client to interact and move cheese
    //             RequestCheeseOwnershipServerRpc(cheeseNetworkObject);

    //             // Only proceed if the object is spawned
    //             try
    //             {
    //                 NetworkObjectReference cheeseReference = cheeseNetworkObject;
    //                 // Further processing with cheeseReference
    //             }
    //             catch (ArgumentException ex)
    //             {
    //                 Debug.LogError($"Error creating NetworkObjectReference for {cheeseNetworkObject.name}: {ex.Message}");
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogWarning("Cheese object is either not found or not spawned.");
    //         }
    //     }
    // }

    // // allow client to interact with cheese
    // [ServerRpc]
    // private void RequestCheeseOwnershipServerRpc(NetworkObjectReference cheeseReference, ServerRpcParams rpcParams = default)
    // {
    //     if (cheeseReference.TryGet(out NetworkObject cheeseObject))
    //     {
    //         cheeseObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
    //     }
    // }

    // get current score on start
    [ServerRpc(RequireOwnership = false)]
    private void RequestScoreServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Pull the up-to-date scores from PointUIManager:
        int currentCatScore   = pointUIManager.GetCatScore();
        int currentMouseScore = pointUIManager.GetMouseScore();

        pointUIManager.UpdateScoreClientRpc(currentCatScore, currentMouseScore);
    }
}
