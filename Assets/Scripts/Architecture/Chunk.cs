using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
[RequireComponent(typeof(TilemapCollider2D))]
public class Chunk : MonoBehaviour
{
    [SerializeField] AnimationCurve layerDarkness;
    public Vector2Int Position { get; private set; }
    private int[,] blocks;
    private const int chunckSize = 16;

    public TilemapCollider2D Collider => collider;
    public Tilemap Tilemap => tilemap;

    new TilemapCollider2D collider;
    Tilemap tilemap;

    public void Initialize(int layer, Vector2Int chunkPosition)
    {
        tilemap = GetComponent<Tilemap>();
        
        Position = chunkPosition;
        blocks = new int[chunckSize, chunckSize];

        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                var idBlock = WorldGenerator.GetBlockID(x + chunkPosition.x, y + chunkPosition.y);

                if (idBlock == 0)
                    continue;

                var t = ScriptableObject.CreateInstance(typeof(Tile)) as Tile;
                t.sprite = WorldGenerator.Instance.blocksData[idBlock].sprite;
                var color = Color.white * layerDarkness.Evaluate(layer);
                color.a = 1;
                t.color = color;
                var tilePos = new Vector3Int(x, y);
                tilemap.SetTile(tilePos, t);

                blocks[x, y] = idBlock;
            }
        }
    }

    public void SetBlock(Vector2Int localPosition, int blockId)
    {
        if (IsValidCoordinate(localPosition))
        {
            blocks[localPosition.x, localPosition.y] = blockId;
        }
    }

    public int GetBlock(Vector2Int localPosition)
    {
        if (IsValidCoordinate(localPosition))
        {
            return blocks[localPosition.x, localPosition.y];
        }
        return -1;
    }

    public Dictionary<Vector3Int, int> GetChunkData()
    {
        Dictionary<Vector3Int, int> chunkData = new Dictionary<Vector3Int, int>();
        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                chunkData[new Vector3Int(x, y, 0)] = blocks[x, y];
            }
        }
        return chunkData;
    }

    public Dictionary<Vector2Int, int> GetAllBlocks()
    {
        Dictionary<Vector2Int, int> allBlocks = new Dictionary<Vector2Int, int>();
        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                if (blocks[x, y] != 0)
                {
                    allBlocks[new Vector2Int(x, y)] = blocks[x, y];
                }
            }
        }
        return allBlocks;
    }

    public void LoadChunkData(Dictionary<Vector2Int, int> chunkData)
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
