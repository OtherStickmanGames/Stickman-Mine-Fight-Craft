using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.IO;


namespace Architecture
{
    public class WorldManager : NetworkBehaviour
    {
        public static WorldManager Instance { get; private set; }
        public int ChunkSize = 16;

        private Dictionary<int, Dictionary<Vector2Int, Chunk>> layers = new Dictionary<int, Dictionary<Vector2Int, Chunk>>();
        private Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();

        private string saveFilePath;

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

        public void SetBlock(int layer, Vector3Int globalPosition, bool isAdding)
        {
            Vector2Int chunkCoord = new Vector2Int(globalPosition.x / ChunkSize, globalPosition.y / ChunkSize);
            Vector3Int localPosition = new Vector3Int(globalPosition.x % ChunkSize, globalPosition.y % ChunkSize, 0);

            if (layers.TryGetValue(layer, out var layerChunks))
            {
                if (layerChunks.TryGetValue(chunkCoord, out var chunk))
                {
                    chunk.SetBlock(localPosition, isAdding);
                }
                else
                {
                    // Чанк не найден, создаём новый чанк
                    chunk = CreateChunk(layer, chunkCoord);
                    chunk.SetBlock(localPosition, isAdding);
                }
            }
            else
            {
                // Слой не найден, создаём новый слой и чанк
                layerChunks = new Dictionary<Vector2Int, Chunk>();
                layers[layer] = layerChunks;
                var chunk = CreateChunk(layer, chunkCoord);
                chunk.SetBlock(localPosition, isAdding);
            }

            // Синхронизация блока с другими клиентами
            SyncBlock(layer, globalPosition, isAdding);
        }

        private Chunk CreateChunk(int layer, Vector2Int chunkCoord)
        {
            GameObject chunkObject = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}_Layer_{layer}");
            chunkObject.transform.parent = transform;
            Chunk chunk = chunkObject.AddComponent<Chunk>();
            chunk.Initialize(chunkCoord);
            layers[layer][chunkCoord] = chunk;
            return chunk;
        }

        public bool GetBlock(int layer, Vector3Int globalPosition)
        {
            Vector2Int chunkCoord = new Vector2Int(globalPosition.x / ChunkSize, globalPosition.y / ChunkSize);
            Vector3Int localPosition = new Vector3Int(globalPosition.x % ChunkSize, globalPosition.y % ChunkSize, 0);

            if (layers.TryGetValue(layer, out var layerChunks))
            {
                if (layerChunks.TryGetValue(chunkCoord, out var chunk))
                {
                    return chunk.GetBlock(localPosition);
                }
            }

            return false;
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
            chunk.Initialize(chunkSize, worldHeight, chunkCoord);
            return chunk;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetBlockServerRpc(Vector2Int chunkCoord, Vector2Int blockCoord, bool isAdding)
        {
            Chunk chunk = GetOrCreateChunk(chunkCoord);
            chunk.SetBlock(blockCoord, isAdding);
            SaveWorldState();
            SetBlockClientRpc(chunkCoord, blockCoord, isAdding);
        }

        [ClientRpc]
        private void SetBlockClientRpc(int layer, Vector3Int globalPosition, bool isAdding)
        {
            if (IsServer) return;
            SetBlock(layer, globalPosition, isAdding);
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

        public void SavePlayerPosition(string clientId, Vector3 position)
        {
            playerPositions[clientId] = position;
            // Сохранение позиций игроков в файл или базу данных
        }

        public Vector3 GetPlayerPosition(string clientId)
        {
            if (playerPositions.TryGetValue(clientId, out var position))
            {
                return position;
            }

            return Vector3.zero; // Если позиция не найдена, возвращаем позицию (0,0,0)
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
            Dictionary<Vector2Int, Dictionary<Vector2Int, bool>> worldState = new Dictionary<Vector2Int, Dictionary<Vector2Int, bool>>();
            foreach (var chunk in chunks)
            {
                worldState[chunk.Key] = chunk.Value.GetAllBlocks();
            }

            string json = JsonUtility.ToJson(new Serialization<Vector2Int, Dictionary<Vector2Int, bool>>(worldState));
            File.WriteAllText(saveFilePath, json);
        }

        public void LoadWorldStateFromStorage()
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                var worldState = JsonUtility.FromJson<Serialization<Vector2Int, Dictionary<Vector2Int, bool>>>(json).ToDictionary();

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

        public void CheckAndSyncChunks()
        {
            foreach (var clientId in playerPositions.Keys)
            {
                Vector3 playerPosition = playerPositions[clientId];
                Vector2Int currentChunkCoord = new Vector2Int((int)playerPosition.x / ChunkSize, (int)playerPosition.y / ChunkSize);

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int chunkCoord = new Vector2Int(currentChunkCoord.x + x, currentChunkCoord.y + y);
                        foreach (var layer in layers.Values)
                        {
                            if (!layer.ContainsKey(chunkCoord))
                            {
                                CreateChunk(0, chunkCoord); // 0 - пример слоя, можно варьировать по необходимости
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (IsServer)
            {
                CheckAndSyncChunks();
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

}

