using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private string inputNameHorizontal;
    [SerializeField] private string inputNameVertical;
    [SerializeField] private Color color;
    
    [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private Transform cameraHolder;

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
            Debug.Log("Escape key pressed!"); // Check if Escape is detected
            ToggleCursor();
        }

        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

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

        // Apply movement while preserving gravity
        rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, moveDirection.z * speed);

        Debug.Log(rb.velocity.y);
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
}
