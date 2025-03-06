using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class Cheese : NetworkBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void RequestOwnership()
    {
        if (IsClient)
        {
            NetworkObject.ChangeOwnership(OwnerClientId);
        }
    }
}
