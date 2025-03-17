using Unity.Netcode;
using UnityEngine;

public class SyncObjectWithRPC : NetworkBehaviour
{
    private void Start()
    {
        if (IsClient && !IsServer) 
        {
            RequestPositionServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        SetPositionClientRpc(transform.position, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void SetPositionClientRpc(Vector3 newPosition, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            transform.position = newPosition;
        }
    }
}
