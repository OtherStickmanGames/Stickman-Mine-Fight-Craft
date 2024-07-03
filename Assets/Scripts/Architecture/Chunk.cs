using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class Chunk : MonoBehaviour
{
    public Sprite sprite;
    public Vector2Int Position { get; private set; }
    private bool[,] blocks;
    private const int chunckSize = 16;

    public Tilemap Tilemap => tilemap;

    Tilemap tilemap;

    public void Initialize(Vector2Int chunkPosition)
    {
        tilemap = GetComponent<Tilemap>();

        Position = chunkPosition;
        blocks = new bool[chunckSize, chunckSize];

        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                var idBlock = WorldGenerator.GetBlockID(x + chunkPosition.x, y + chunkPosition.y);

                //if (idBlock < 0)
                //    continue;

                var t = ScriptableObject.CreateInstance(typeof(Tile)) as Tile;
                t.sprite = sprite;// blockData[idBlock].sprite;
                var tilePos = new Vector3Int(x, y);
                tilemap.SetTile(tilePos, t);
                print(tilePos);
            }
        }
    }

    public void SetBlock(Vector2Int localPosition, bool isAdding)
    {
        if (IsValidCoordinate(localPosition))
        {
            blocks[localPosition.x, localPosition.y] = isAdding;
        }
    }

    public bool GetBlock(Vector2Int localPosition)
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
        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                chunkData[new Vector3Int(x, y, 0)] = blocks[x, y];
            }
        }
        return chunkData;
    }

    public Dictionary<Vector2Int, bool> GetAllBlocks()
    {
        Dictionary<Vector2Int, bool> allBlocks = new Dictionary<Vector2Int, bool>();
        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                if (blocks[x, y])
                {
                    allBlocks[new Vector2Int(x, y)] = blocks[x, y];
                }
            }
        }
        return allBlocks;
    }

    public void LoadChunkData(Dictionary<Vector2Int, bool> chunkData)
    {
        foreach (var kvp in chunkData)
        {
            SetBlock(kvp.Key, kvp.Value);
        }
    }

    private bool IsValidCoordinate(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < chunckSize && coord.y >= 0 && coord.y < chunckSize;
    }
}
