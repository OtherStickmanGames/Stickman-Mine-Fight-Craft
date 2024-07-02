using Architecture;
using Unity.Netcode;
using UnityEngine;

public class BlockManager : NetworkBehaviour
{
    public static BlockManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to handle block changes requested by the player
    public void RequestBlockChange(Vector2Int position, bool isAdding)
    {
        if (IsServer)
        {
            // If called on the server, directly update the block
            SetBlock(position, isAdding);
        }
        else
        {
            // If called on the client, send a request to the server
            RequestBlockChangeServerRpc(position, isAdding);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBlockChangeServerRpc(Vector2Int position, bool isAdding)
    {
        SetBlock(position, isAdding);
    }

    // Method to set a block on the server and sync with clients
    private void SetBlock(Vector2Int position, bool isAdding)
    {
        Chunk chunk = WorldManager.Instance.GetOrCreateChunk(position);
        if (chunk != null)
        {
            chunk.SetBlock(position, isAdding);
            SyncBlockClientRpc(position, isAdding);
        }
    }

    [ClientRpc]
    private void SyncBlockClientRpc(Vector2Int position, bool isAdding)
    {
        Chunk chunk = WorldManager.Instance.GetOrCreateChunk(position);
        if (chunk != null)
        {
            chunk.SetBlock(position, isAdding);
        }
    }

    // Save world state (can be called on the server)
    public void SaveWorldState()
    {
        //WorldManager.Instance.SaveWorldState();
    }

    // Load world state (can be called on the server)
    public void LoadWorldState()
    {
        //WorldManager.Instance.LoadWorldState();
    }
}
