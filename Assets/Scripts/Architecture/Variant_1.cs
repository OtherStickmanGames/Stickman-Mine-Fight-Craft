using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Netcode;

public class Variant_1 : NetworkBehaviour
{
    public int chunkSize = 16;
    public int worldHeight = 256;
    public GameObject chunkPrefab;
    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

    private string saveFilePath;

    private void Awake()
    {

        saveFilePath = Path.Combine(Application.persistentDataPath, "worldState.json");
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            LoadWorldStateFromStorage();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        LoadWorldState(clientId);
    }

    public void LoadWorldState(ulong clientId)
    {
        foreach (var chunk in chunks.Values)
        {
            SyncChunkWithClient(chunk, clientId);
        }
    }

    private void SyncChunkWithClient(Chunk chunk, ulong clientId)
    {
        foreach (var block in chunk.GetAllBlocks())
        {
            SetBlockClientRpc(chunk.Position, block.Key, block.Value, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
        }
    }

    public Chunk GetOrCreateChunk(Vector2Int chunkCoord)
    {
        if (!chunks.TryGetValue(chunkCoord, out Chunk chunk))
        {
            chunk = GenerateChunk(chunkCoord);
            chunks[chunkCoord] = chunk;
        }
        return chunk;
    }

    private Chunk GenerateChunk(Vector2Int chunkCoord)
    {
        GameObject chunkObject = Instantiate(chunkPrefab);
        chunkObject.transform.position = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * worldHeight);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.Initialize(0, chunkCoord);
        return chunk;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBlockServerRpc(Vector2Int chunkCoord, Vector2Int blockCoord, int blockId)
    {
        Chunk chunk = GetOrCreateChunk(chunkCoord);
        chunk.SetBlock(blockCoord, blockId);
        SaveWorldState();
        SetBlockClientRpc(chunkCoord, blockCoord, blockId);
    }

    [ClientRpc]
    private void SetBlockClientRpc(Vector2Int chunkCoord, Vector2Int blockCoord, int blockId, ClientRpcParams clientRpcParams = default)
    {
        Chunk chunk = GetOrCreateChunk(chunkCoord);
        chunk.SetBlock(blockCoord, blockId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChunkSpawnServerRpc(Vector2Int chunkCoord)
    {
        if (!chunks.ContainsKey(chunkCoord))
        {
            Chunk chunk = GenerateChunk(chunkCoord);
            chunks[chunkCoord] = chunk;
            SyncChunkWithAllClients(chunk);
        }
    }

    private void SyncChunkWithAllClients(Chunk chunk)
    {
        foreach (var block in chunk.GetAllBlocks())
        {
            SetBlockClientRpc(chunk.Position, block.Key, block.Value);
        }
    }

    public void OnPlayerPositionChanged(Vector3 playerPosition)
    {
        Vector2Int playerChunkCoord = new Vector2Int(Mathf.FloorToInt(playerPosition.x / chunkSize), Mathf.FloorToInt(playerPosition.y / worldHeight));
        CheckAndSyncChunks(playerChunkCoord);
    }

    private void CheckAndSyncChunks(Vector2Int playerChunkCoord)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + x, playerChunkCoord.y + y);
                if (!chunks.ContainsKey(chunkCoord))
                {
                    RequestChunkSpawnServerRpc(chunkCoord);
                }
            }
        }
    }

    public void SaveWorldState()
    {
        Dictionary<Vector2Int, Dictionary<Vector2Int, int>> worldState = new Dictionary<Vector2Int, Dictionary<Vector2Int, int>>();
        foreach (var chunk in chunks)
        {
            worldState[chunk.Key] = chunk.Value.GetAllBlocks();
        }

        string json = JsonUtility.ToJson(new Serialization<Vector2Int, Dictionary<Vector2Int, int>>(worldState));
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadWorldStateFromStorage()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            var worldState = JsonUtility.FromJson<Serialization<Vector2Int, Dictionary<Vector2Int, int>>>(json).ToDictionary();

            foreach (var chunk in worldState)
            {
                Chunk newChunk = GetOrCreateChunk(chunk.Key);
                foreach (var block in chunk.Value)
                {
                    newChunk.SetBlock(block.Key, block.Value);
                }
            }
        }
    }

    [System.Serializable]
    private class Serialization<TKey, TValue>
    {
        public List<TKey> keys;
        public List<TValue> values;

        public Serialization(Dictionary<TKey, TValue> dictionary)
        {
            keys = new List<TKey>(dictionary.Keys);
            values = new List<TValue>(dictionary.Values);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                result[keys[i]] = values[i];
            }
            return result;
        }
    }


}
