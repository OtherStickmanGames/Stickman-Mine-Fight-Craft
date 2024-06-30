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

        private Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();

        private void Start()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObject != null)
            {
                playerObject.GetComponent<Player>().OnPositionChanged += OnPlayerPositionChanged;
            }
        }

        private void OnPlayerPositionChanged(Vector3 playerPosition)
        {
            Vector2Int currentChunkCoord = GetChunkCoordFromPosition(playerPosition);

            for (int xOffset = -viewDistance; xOffset <= viewDistance; xOffset++)
            {
                for (int yOffset = -viewDistance; yOffset <= viewDistance; yOffset++)
                {
                    Vector2Int chunkCoord = currentChunkCoord + new Vector2Int(xOffset, yOffset);
                    if (!loadedChunks.ContainsKey(chunkCoord))
                    {
                        GenerateChunk(chunkCoord);
                        RequestChunkSpawnClientRpc(chunkCoord);
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
        }

        [ClientRpc]
        private void RequestChunkSpawnClientRpc(Vector2Int chunkCoord)
        {
            if (IsServer) return;

            Vector3 chunkPosition = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, 0);
            GameObject chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
            loadedChunks[chunkCoord] = chunk;
        }
    }
}

