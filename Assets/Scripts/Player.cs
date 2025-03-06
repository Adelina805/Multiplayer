using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class Player : NetworkBehaviour
{
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
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset vertical velocity
            rb.AddForce(Vector3.up * jumpForce / Time.fixedDeltaTime, ForceMode.Impulse);
        }

        if (!IsGrounded()) 
        {
            Debug.Log("is not on da ground");
            rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration); // Faster fall
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

    // NEW: Request ownership of cheese when colliding with it**
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Cheese"))
        {
            NetworkObject cheeseObject = collision.gameObject.GetComponent<NetworkObject>();
            if (cheeseObject != null && cheeseObject.IsOwnedByServer)
            {
                RequestCheeseOwnershipServerRpc(cheeseObject);
            }
        }
    }

    [ServerRpc]
    private void RequestCheeseOwnershipServerRpc(NetworkObjectReference cheeseReference, ServerRpcParams rpcParams = default)
    {
        if (cheeseReference.TryGet(out NetworkObject cheeseObject))
        {
            cheeseObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        }
    }
}
