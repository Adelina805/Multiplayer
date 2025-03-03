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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        renderer = GetComponentInChildren<Renderer>();
        renderer.material.color = color;
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
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return; // Prevents non-local players from being moved by input

        rb.velocity = new Vector3(inputHorizontal * speed, rb.velocity.y, inputVertical * speed);
    }
}
