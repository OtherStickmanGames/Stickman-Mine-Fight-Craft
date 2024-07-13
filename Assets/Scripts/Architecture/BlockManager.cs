using System;
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

    private void Start()
    {
        
    }

    

    // Method to handle block changes requested by the player
    public void RequestBlockChange(int layer, Vector2Int position, int blockID)
    {
        if (IsServer)
        {
            // If called on the server, directly update the block
            SetBlock(layer, position, blockID);
        }
        else
        {
            // If called on the client, send a request to the server
            RequestBlockChangeServerRpc(layer, position, blockID);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBlockChangeServerRpc(int layer, Vector2Int position, int blockID)
    {
        SetBlock(layer, position, blockID);
    }

    public void SetBlock(int layer, Vector2 position, int blockID)
    {
        Chunk chunk = WorldManager.Instance.GetChunck(layer, position);
        if (chunk != null)
        {
            chunk.SetBlock(position, blockID);
            SyncBlock(layer, position, blockID);
        }
    }

    private void SyncBlock(int layer, Vector2 worldPosition, int blockID)
    {
        WorldManager.Instance.SetBlockServerRpc(layer, worldPosition, blockID);
    }

    public void SwitchLayer(int layer, LayerSwitchDir dir)
    {
        WorldManager.Instance.SwitchLayer(layer, dir);
    }

    Vector2 checkablePos;
    int checkableBlockId;
    public bool CheckAvailableMoveToLayer(int layer, Vector2 worldPosition, int height = 2)
    {
        if (layer < 0 || layer >= WorldManager.Instance.numLayers)
            return false;

        for (int i = 0; i < height; i++)
        {
            checkablePos = worldPosition + Vector2.up * i;
            checkableBlockId = WorldManager.Instance.GetBlock(layer, checkablePos);
            if (checkableBlockId > 0)
            {
                return false;
            }
        }

        return true;
    }
}
