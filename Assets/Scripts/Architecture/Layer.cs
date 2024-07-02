using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{

    public class Layer
    {
        private int chunkSize;
        private Dictionary<Vector2Int, Chunk> chunks;

        public Layer(int chunkSize)
        {
            this.chunkSize = chunkSize;
            chunks = new Dictionary<Vector2Int, Chunk>();
        }

        public bool HasChunk(Vector2Int chunkCoord)
        {
            return chunks.ContainsKey(chunkCoord);
        }

        public void SetChunk(Vector2Int chunkCoord, Chunk chunk)
        {
            chunks[chunkCoord] = chunk;
        }

        public void SetBlock(Vector3Int position, bool isAdding)
        {
            Vector2Int chunkCoord = new Vector2Int(position.x / chunkSize, position.y / chunkSize);
            if (chunks.ContainsKey(chunkCoord))
            {
                chunks[chunkCoord].SetBlock(new Vector3Int(position.x % chunkSize, position.y % chunkSize, position.z), isAdding);
            }
        }

        public bool GetBlock(Vector3Int position)
        {
            Vector2Int chunkCoord = new Vector2Int(position.x / chunkSize, position.y / chunkSize);
            if (chunks.ContainsKey(chunkCoord))
            {
                return chunks[chunkCoord].GetBlock(new Vector3Int(position.x % chunkSize, position.y % chunkSize, position.z));
            }
            return false;
        }

        public Dictionary<Vector2Int, bool[,]> GetAllChunksState()
        {
            Dictionary<Vector2Int, bool[,]> allChunksState = new Dictionary<Vector2Int, bool[,]>();
            foreach (var chunk in chunks)
            {
                allChunksState[chunk.Key] = chunk.Value.GetBlocksState();
            }
            return allChunksState;
        }

        public void SetAllChunksState(Dictionary<Vector2Int, bool[,]> allChunksState)
        {
            foreach (var chunkState in allChunksState)
            {
                if (!chunks.ContainsKey(chunkState.Key))
                {
                    chunks[chunkState.Key] = new Chunk(chunkSize);
                }
                chunks[chunkState.Key].SetBlocksState(chunkState.Value);
            }
        }
    }



}
