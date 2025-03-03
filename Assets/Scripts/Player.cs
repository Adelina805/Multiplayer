using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private string inputNameHorizontal;
    [SerializeField] private string inputNameVertical;
    [SerializeField] private Color color;

    [SerializeField] private CinemachineVirtualCamera camera;

    private Rigidbody rb;
    private Renderer renderer;

    private float inputHorizontal;
    private float inputVertical;

// // trying to get camera to move
    public float sensX;
    public float sensY;
    public Transform orientation;
    public float xRotation;
    public float yRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        renderer = GetComponentInChildren<Renderer>();
        renderer.material.color = color;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            camera.Priority = 1;
        } else 
        {
            camera.Priority = 0;
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // Only allow movement for the local player

        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        Debug.Log($"Player {OwnerClientId}: Horizontal={inputHorizontal}, Vertical={inputVertical}");

        //get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        Debug.Log($"Mouse Input: Horizontal = {mouseX}, Vertical={mouseY}");

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return; // Prevents non-local players from being moved by input

        rb.velocity = new Vector3(inputHorizontal * speed, rb.velocity.y, inputVertical * speed);
    }
}
