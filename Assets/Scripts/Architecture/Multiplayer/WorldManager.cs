using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Architecture
{
    public class WorldManager : NetworkBehaviour
    {
        public Chunk chunkPrefab; // Префаб чанка для создания новых экземпляров

        public static WorldManager Instance { get; private set; }
        public int chunkSize = 16;
        public int viewDistance = 3;
        public int numLayers = 4;

        private Dictionary<int, Dictionary<Vector2Int, Chunk>> layers = new Dictionary<int, Dictionary<Vector2Int, Chunk>>();
        private Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();
        Player player;
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
        }

        private void Start()
        {
            // Определите путь для сохранения состояния мира
            saveFilePath = Application.dataPath + "/world_save.json";

            for (int i = 0; i < numLayers; i++)
            {
                layers[i] = new Dictionary<Vector2Int, Chunk>();
            }

            // Загрузить состояние мира при запуске сервера
            if (IsServer)
            {
                //LoadWorldState();
            }
            else
            {
                Player.onOwnerSpawn.AddListener(Owner_Spawned);
            }
        }

        private void Owner_Spawned(Player owner)
        {
            player = owner;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }


        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObject != null)
            {
                //playerObject.GetComponent<Player>().OnPositionChanged += OnPlayerPositionChanged;
                Debug.Log($"Player object found for client {clientId}");
            }
            else
            {
                Debug.LogError($"Player object not found for client {clientId}");
            }
        }


        private void CheckViewedChuncks()
        {
            var playerPosition = player.transform.position;
            Vector2Int currentChunkCoord = GetChunkCoordFromPosition(playerPosition);

            for (int i = 0; i < numLayers; i++)
            {
                for (int xOffset = -viewDistance; xOffset <= viewDistance; xOffset++)
                {
                    for (int yOffset = -viewDistance; yOffset <= viewDistance; yOffset++)
                    {
                        Vector2Int chunkCoord = currentChunkCoord + new Vector2Int(xOffset, yOffset);

                        var chuncks = layers[i];
                        if (!chuncks.ContainsKey(chunkCoord))
                        {
                            var chunk = GenerateChunk(i, chunkCoord);
                            chuncks[chunkCoord] = chunk;

                            UpdateChunckLayer(chunk, i);

                            RequestChunckDataServerRpc(i, chunkCoord * chunkSize);
                            return;
                        }
                        else
                        {
                            var chunck = chuncks[chunkCoord];
                            if (!chunck.isActiveAndEnabled)
                            {
                                chunck.gameObject.SetActive(true);
                                RequestChunckDataServerRpc(i, chunkCoord * chunkSize);
                                UpdateChunckLayer(chunck, i);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateChunckLayer(Chunk chunk, int chunkLayer)
        {
            var playerLayer = player.Controller.Layer;
            if (playerLayer > chunkLayer)
            {
                chunk.Hide();
            }
            else
            {
                var colorLayer = chunkLayer - playerLayer;
                chunk.ChangeColor(colorLayer);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestChunckDataServerRpc(int layer, Vector2Int chunckPosition, ServerRpcParams serverRpcParams = default)
        {
            var filePath = ChunckFilePath(layer, chunckPosition);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var chunckData = JsonUtility.FromJson<ChunkData>(json);

                var clientPrcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
                    }
                };

                var blocksPos = chunckData.blocks.Select(b => new Vector2Int(b.x, b.y)).ToArray();
                var blocksId = chunckData.blocks.Select(b => b.id).ToArray();
                ReceiveChunckDataClientRpc(layer, chunckPosition, blocksPos, blocksId, clientPrcParams);
            }
        }

        [ClientRpc]
        public void ReceiveChunckDataClientRpc(int layer, Vector2Int chunckPos, Vector2Int[] blocksPos, int[] blocksId, ClientRpcParams clientRpcParams = default)
        {
            var chunck = GetChunck(layer, chunckPos);
            var length = blocksId.Length;
            for (int i = 0; i < length; i++)
            {
                chunck.SetBlock(blocksPos[i], blocksId[i]);
            }
        }

        private Vector2Int GetChunkCoordFromPosition(Vector3 anyPosition)
        {
            int x = Mathf.FloorToInt(anyPosition.x / chunkSize);
            int y = Mathf.FloorToInt(anyPosition.y / chunkSize);
            return new Vector2Int(x, y);
        }

        //private void CheckAndSyncChunks(Vector2Int playerChunkCoord)
        //{
        //    for (int x = -1; x <= 1; x++)
        //    {
        //        for (int y = -1; y <= 1; y++)
        //        {
        //            Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + x, playerChunkCoord.y + y);
        //            if (!chunks.ContainsKey(chunkCoord))
        //            {
        //                RequestChunkSpawnServerRpc(chunkCoord);
        //            }
        //        }
        //    }
        //}

        public void LoadWorldState()
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                WorldStateData worldStateData = JsonUtility.FromJson<WorldStateData>(json);

                foreach (var layerData in worldStateData.layers)
                {
                    int layerIndex = layerData.layerIndex;

                    if (!layers.ContainsKey(layerIndex))
                    {
                        layers[layerIndex] = new Dictionary<Vector2Int, Chunk>();
                    }

                    foreach (var chunkData in layerData.chunks)
                    {
                        Vector2Int chunkCoord = new Vector2Int(chunkData.x, chunkData.y);

                        // Создаем новый чанк через префаб
                        Chunk chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
                        chunk.Initialize(layerIndex, chunkCoord);

                        // Заполняем чанк блоками
                        for (int x = 0; x < chunkSize; x++)
                        {
                            for (int y = 0; y < chunkSize; y++)
                            {
                                //chunk.SetBlock(chunkCoord, chunkData.blocks[x, y]);
                            }
                        }

                        layers[layerIndex][chunkCoord] = chunk;
                    }
                }
            }
        }


        public void SetBlock(int layer, Vector3Int globalPosition, int blockID)
        {
            Vector2Int chunkCoord = new Vector2Int(globalPosition.x / chunkSize, globalPosition.y / chunkSize);
            Vector2Int localPosition = new Vector2Int(globalPosition.x % chunkSize, globalPosition.y % chunkSize);

            if (layers.TryGetValue(layer, out var layerChunks))
            {
                if (layerChunks.TryGetValue(chunkCoord, out var chunk))
                {
                    chunk.SetBlock(localPosition, blockID);
                }
                else
                {
                    // Чанк не найден, создаём новый чанк
                    chunk = CreateChunk(layer, chunkCoord);
                    chunk.SetBlock(localPosition, blockID);
                }
            }
            else
            {
                // Слой не найден, создаём новый слой и чанк
                layerChunks = new Dictionary<Vector2Int, Chunk>();
                layers[layer] = layerChunks;
                var chunk = CreateChunk(layer, chunkCoord);
                chunk.SetBlock(localPosition, blockID);
            }

            // Синхронизация блока с другими клиентами
            SyncBlock(layer, globalPosition, blockID);
        }

        private void SyncBlock(int layer, Vector3Int globalPosition, int blockID)
        {
            // Логика синхронизации блока с другими клиентами
            // Вызывается метод RPC для синхронизации блока
            //SetBlockClientRpc(layer, globalPosition, blockID);
        }

        private Chunk CreateChunk(int layer, Vector2Int chunkCoord)
        {
            GameObject chunkObject = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}_Layer_{layer}");
            chunkObject.transform.parent = transform;
            Chunk chunk = chunkObject.AddComponent<Chunk>();
            chunk.Initialize(layer, chunkCoord);
            layers[layer][chunkCoord] = chunk;
            return chunk;
        }

        public int GetBlock(int layer, Vector2 globalPosition)
        {
            var chunck = GetChunck(layer, globalPosition);
            return chunck?.GetBlock(globalPosition) ?? 88888888;
        }

        
        private void SyncChunkWithClient(Chunk chunk, ulong clientId)
        {
            //foreach (var block in chunk.GetAllBlocks())
            //{
            //    SetBlockClientRpc(chunk.Position, block.Key, block.Value, new ClientRpcParams
            //    {
            //        Send = new ClientRpcSendParams
            //        {
            //            TargetClientIds = new[] { clientId }
            //        }
            //    });
            //}
        }

        public Chunk GetOrCreateChunk(int layer, Vector2Int chunkCoord)
        {
            var chunks = layers[layer];
            if (!chunks.TryGetValue(chunkCoord, out Chunk chunk))
            {
                chunk = GenerateChunk(layer, chunkCoord);
                chunks[chunkCoord] = chunk;
            }
            return chunk;
        }

        Vector2Int chunckKey;
        public Chunk GetChunck(int layer, Vector2 globalPos)
        {
            chunckKey.x = Mathf.FloorToInt(globalPos.x / chunkSize);
            chunckKey.y = Mathf.FloorToInt(globalPos.y / chunkSize);
            var chunks = layers[layer];
            if (chunks.TryGetValue(chunckKey, out Chunk chunk))
            {
                return chunk;
            }
            return null;
        }

        public List<Chunk> GetAllChuncksByLayer(int layer)
        {
            return layers[layer].Select(kv => kv.Value).ToList();
        }

        public void HideLayer(int layer)
        {
            var chuncks = GetAllChuncksByLayer(layer);
            foreach (var chunck in chuncks)
            {
                chunck.Hide();
            }
        }

        public void SwitchLayer(int layer, LayerSwitchDir dir)
        {
            if (dir == LayerSwitchDir.Next)
            {
                HideLayer(layer - 1);

                for (int i = 0; i < numLayers; i++)
                {
                    if (i == layer - 1)
                        continue;

                    var chuncks = GetAllChuncksByLayer(i);
                    foreach (var chunck in chuncks)
                    {
                        var colorLayer = i - layer;
                        chunck.ChangeColor(colorLayer);
                    }
                }
            }
        }

        private Chunk GenerateChunk(int layer, Vector2Int chunkKey)
        {
            var chunk = Instantiate(chunkPrefab, transform);
            chunk.name = $"Chunk-Layer:{layer}";
            chunk.transform.position = new Vector3(chunkKey.x * chunkSize, chunkKey.y * chunkSize, layer);
            var chunkPosition = new Vector2Int(chunkKey.x * chunkSize, chunkKey.y * chunkSize);
            chunk.Initialize(layer, chunkPosition);
            //Debug.Log($"Chunck generated: {chunkKey}");
            return chunk;
        }

        string ChunckFilePath(int layer, Vector2Int chunckPosition)
        {
            return $"{Application.dataPath}/Data/Server/Chunck{chunckPosition}Layer{layer}.json";
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetBlockServerRpc(int layer, Vector2 worldPosition, int blockID, ServerRpcParams serverRpcParams = default)
        {
            Vector2Int chunckPosition = new(Mathf.FloorToInt(worldPosition.x / chunkSize), Mathf.FloorToInt(worldPosition.y / chunkSize));
            chunckPosition *= chunkSize;
            Vector2Int blockPosition = new(Mathf.FloorToInt(worldPosition.x - chunckPosition.x), Mathf.FloorToInt(worldPosition.y - chunckPosition.y));

            var filePath = ChunckFilePath(layer, chunckPosition);
            if (!File.Exists(filePath))
            {
                var chunckData = new ChunkData(chunckPosition);
                chunckData.blocks.Add(new BlockData(blockPosition, blockID));
                var json = JsonUtility.ToJson(chunckData);
                File.WriteAllText(filePath, json);
            }
            else
            {
                var json = File.ReadAllText(filePath);
                var chunckData = JsonUtility.FromJson<ChunkData>(json);
                var blockData = chunckData.blocks.Find(b => b.x == blockPosition.x && b.y == blockPosition.y);
                if (blockData == null)
                {
                    chunckData.blocks.Add(new BlockData(blockPosition, blockID));
                }
                else
                {
                    blockData.id = blockID;
                }
                json = JsonUtility.ToJson(chunckData);
                File.WriteAllText(filePath, json);
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            CheckChunckChangeNearestPlayers(layer, worldPosition, blockID, serverRpcParams.Receive.SenderClientId);
        }

        void CheckChunckChangeNearestPlayers(int layer, Vector2 worldPosition, int blockID, ulong clientID)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                if (client.Key == clientID)
                    continue;

                var playerObject = client.Value.PlayerObject;
                if (!playerObject)
                    continue;

                //print($"{client.Key} == {client.Value.PlayerObject}");
                var dist = Vector2.Distance(playerObject.transform.position, worldPosition);
                if (dist < (viewDistance + 1) * chunkSize)
                {
                    var crp = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { client.Key }
                        }
                    };
                    SetBlockClientRpc(layer, worldPosition, blockID, crp);
                }
            }
        }

        [ClientRpc]
        private void SetBlockClientRpc(int layer, Vector2 worldPosition, int blockID, ClientRpcParams clientRpcParams = default)
        {
            var chunck = GetChunck(layer, worldPosition);
            if (chunck)
            {
                chunck.SetBlock(worldPosition, blockID);
            }
            //SetBlock(layer, globalPosition, blockID);
        }

        [ClientRpc]
        private void SetBlockClientRpc(int layer, Vector2Int chunkCoord, Vector2Int blockCoord, int blockID)
        {
            Chunk chunk = GetOrCreateChunk(layer, chunkCoord);
            chunk.SetBlock(blockCoord, blockID);
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
  

        public void CheckAndSyncChunks()
        {
            foreach (var clientId in playerPositions.Keys)
            {
                Vector3 playerPosition = playerPositions[clientId];
                Vector2Int currentChunkCoord = new ((int)playerPosition.x / chunkSize, (int)playerPosition.y / chunkSize);

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int chunkCoord = new (currentChunkCoord.x + x, currentChunkCoord.y + y);
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
            if (player)
            {
                CheckViewedChuncks();
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


    [Serializable]
    public class WorldStateData
    {
        public List<LayerData> layers = new List<LayerData>();
    }

    [Serializable]
    public class LayerData
    {
        public int layerIndex;
        public List<ChunkData> chunks = new List<ChunkData>();
    }

    [Serializable]
    public class ChunkData
    {
        public int x, y;
        public List<BlockData> blocks;

        public ChunkData(Vector2Int chunckPos)
        {
            x = chunckPos.x;
            y = chunckPos.y;
            blocks = new List<BlockData>();
        }

        private ChunkData() { }
    }

    [Serializable]
    public class BlockData
    {
        public int x, y, id;

        public BlockData(int x, int y, int id)
        {
            this.x = x;
            this.y = y;
            this.id = id;
        }

        public BlockData(Vector2Int pos, int id)
        {
            this.x = pos.x;
            this.y = pos.y;
            this.id = id;
        }

        private BlockData() { }
    }
}


public enum LayerSwitchDir
{
    Next, Prev
}
