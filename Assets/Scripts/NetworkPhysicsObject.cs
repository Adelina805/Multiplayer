using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//test

public class NetworkPhysicsObject : NetworkBehaviour
{
    public Rigidbody rb;
    private NetworkVariable<Vector3> velocity = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> angularVelocity = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsOwner)
        {
            rb.isKinematic = true; // Prevents clients from simulating physics
            velocity.OnValueChanged += (oldVal, newVal) => rb.velocity = newVal;
            angularVelocity.OnValueChanged += (oldVal, newVal) => rb.angularVelocity = newVal;
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            velocity.Value = rb.velocity;
            angularVelocity.Value = rb.angularVelocity;
        }
    }
}
