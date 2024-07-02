using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Vector2Int ChunkPosition { get; private set; }
    private bool[,] blocks;
    private const int ChunkSize = 16;

    public void Initialize(Vector2Int chunkPosition)
    {
        ChunkPosition = chunkPosition;
        blocks = new bool[ChunkSize, ChunkSize];
    }

    public void SetBlock(Vector3Int localPosition, bool isAdding)
    {
        if (IsValidCoordinate(localPosition))
        {
            blocks[localPosition.x, localPosition.y] = isAdding;
        }
    }

    public bool GetBlock(Vector3Int localPosition)
    {
        if (IsValidCoordinate(localPosition))
        {
            return blocks[localPosition.x, localPosition.y];
        }
        return false;
    }

    public Dictionary<Vector3Int, bool> GetChunkData()
    {
        Dictionary<Vector3Int, bool> chunkData = new Dictionary<Vector3Int, bool>();
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                chunkData[new Vector3Int(x, y, 0)] = blocks[x, y];
            }
        }
        return chunkData;
    }

    public void LoadChunkData(Dictionary<Vector3Int, bool> chunkData)
    {
        foreach (var kvp in chunkData)
        {
            SetBlock(kvp.Key, kvp.Value);
        }
    }

    private bool IsValidCoordinate(Vector3Int coord)
    {
        return coord.x >= 0 && coord.x < ChunkSize && coord.y >= 0 && coord.y < ChunkSize;
    }
}
