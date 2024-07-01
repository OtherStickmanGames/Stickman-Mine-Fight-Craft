using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace Architecture
{
    public class WorldManager : NetworkBehaviour
    {
        public GameObject chunkPrefab;
        public int chunkSize = 16;
        public int viewDistance = 3;
        public float syncDistanceThreshold = 50f; // Расстояние для синхронизации чанков

        private Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<ulong, Vector3> playerPositions = new Dictionary<ulong, Vector3>();

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
                var player = playerObject.GetComponent<Player>();
                player.OnPositionChanged += (position) => OnPlayerPositionChanged(clientId, position);
                Debug.Log($"Player object found for client {clientId}");
            }
            else
            {
                Debug.LogError($"Player object not found for client {clientId}");
            }
        }

        private void OnPlayerPositionChanged(ulong clientId, Vector3 playerPosition)
        {
            Debug.Log($"Player position changed: {playerPosition}");
            playerPositions[clientId] = playerPosition;
            Vector2Int currentChunkCoord = GetChunkCoordFromPosition(playerPosition);

            for (int xOffset = -viewDistance; xOffset <= viewDistance; xOffset++)
            {
                for (int yOffset = -viewDistance; yOffset <= viewDistance; yOffset++)
                {
                    Vector2Int chunkCoord = currentChunkCoord + new Vector2Int(xOffset, yOffset);
                    if (!loadedChunks.ContainsKey(chunkCoord))
                    {
                        GenerateChunk(chunkCoord);
                    }
                }
            }
        }

        private Vector2Int GetChunkCoordFromPosition(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / chunkSize);
            int y = Mathf.FloorToInt(position.y / chunkSize);
            return new Vector2Int(x, y);
        }

        private void GenerateChunk(Vector2Int chunkCoord)
        {
            Vector3 chunkPosition = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 0);
            GameObject chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
            NetworkObject networkObject = chunk.GetComponent<NetworkObject>();
            networkObject.Spawn();
            loadedChunks[chunkCoord] = chunk;
            Debug.Log($"Chunk generated at {chunkCoord}");
        }

        [ClientRpc]
        private void RequestChunkSpawnClientRpc(Vector2Int chunkCoord, ulong clientId)
        {
            if (IsServer) return;

            Vector3 chunkPosition = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 0);
            if (!loadedChunks.ContainsKey(chunkCoord))
            {
                GameObject chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
                loadedChunks[chunkCoord] = chunk;
                Debug.Log($"Chunk spawned on client at {chunkCoord}");
            }
        }

        private void CheckAndSyncChunks()
        {
            foreach (var clientId in playerPositions.Keys)
            {
                Vector3 playerPosition = playerPositions[clientId];
                Vector2Int playerChunkCoord = GetChunkCoordFromPosition(playerPosition);

                foreach (var otherClientId in playerPositions.Keys)
                {
                    if (clientId != otherClientId)
                    {
                        Vector3 otherPlayerPosition = playerPositions[otherClientId];
                        float distance = Vector3.Distance(playerPosition, otherPlayerPosition);

                        if (distance < syncDistanceThreshold)
                        {
                            Vector2Int otherPlayerChunkCoord = GetChunkCoordFromPosition(otherPlayerPosition);
                            if (!loadedChunks.ContainsKey(otherPlayerChunkCoord))
                            {
                                RequestChunkSpawnClientRpc(otherPlayerChunkCoord, otherClientId);
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
    }
}

