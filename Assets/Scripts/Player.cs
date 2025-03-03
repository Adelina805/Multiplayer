using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private string inputNameHorizontal;
    [SerializeField] private string inputNameVertical;
    [SerializeField] private Color color;
    
    [SerializeField] private CinemachineVirtualCamera camera;

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
        
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor to screen center
            Cursor.visible = false; // Hide cursor
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

        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        // Mouse look
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player left and right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevents flipping

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 moveDirection = transform.forward * inputVertical + transform.right * inputHorizontal;
    
        rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y - 9.81f * Time.fixedDeltaTime, moveDirection.z * speed);

        Debug.Log($"Velocity Y: {rb.velocity.y}");

    }

}
