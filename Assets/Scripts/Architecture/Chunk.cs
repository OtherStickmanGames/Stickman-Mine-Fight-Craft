using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using Architecture;

//[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
[RequireComponent(typeof(TilemapCollider2D))]
public class Chunk : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] private new TilemapCollider2D collider;
    [SerializeField] AnimationCurve layerDarkness;
    
    public int[,] blocks;

    public Vector2Int Position { get; private set; }
    public TilemapCollider2D Collider => collider;
    public Tilemap Tilemap => tilemap;

    Transform player;
    Color layerColor;

    const int chunckSize = 16;

    public void Initialize(int layer, Vector2Int chunkPosition)
    {
        Position = chunkPosition;
        blocks = new int[chunckSize, chunckSize];

        gameObject.layer = LayerMask.NameToLayer($"LAYER_{layer}");

        layerColor = Color.white * layerDarkness.Evaluate(layer);
        layerColor.a = 1;
        tilemap.color = layerColor;

        for (int x = 0; x < chunckSize; x++)
        {
            for (int y = 0; y < chunckSize; y++)
            {
                var idBlock = ProceduralGenerator.GetBlockID(x + chunkPosition.x, y + chunkPosition.y, layer);

                if (idBlock == 0)
                    continue;

                var t = ScriptableObject.CreateInstance<Tile>();
                t.sprite = ProceduralGenerator.Instance.blocksData[idBlock].sprite;
                //t.color = layerColor;
                var tilePos = new Vector3Int(x, y);
                tilemap.SetTile(tilePos, t);

                blocks[x, y] = idBlock;
            }
        }

        player = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
    }

    Vector2Int blockPosition;
    public void SetBlock(Vector2 globalPosition, int blockId)
    {
        blockPosition.x = Mathf.FloorToInt(globalPosition.x - transform.position.x);
        blockPosition.y = Mathf.FloorToInt(globalPosition.y - transform.position.y);
        if (IsValidCoordinate(blockPosition))
        {
            blocks[blockPosition.x, blockPosition.y] = blockId;
            UpdateTilemap(globalPosition, blockId);
        }
    }

    public int GetBlock(Vector2 worldPosition)
    {
        blockPosition.x = Mathf.FloorToInt(worldPosition.x - transform.position.x);
        blockPosition.y = Mathf.FloorToInt(worldPosition.y - transform.position.y);
        return blocks[blockPosition.x, blockPosition.y];
    }

    public void ChangeColor(int colorLayer)
    {
        var startColor = tilemap.color;
        layerColor = Color.white * layerDarkness.Evaluate(colorLayer);
        layerColor.a = tilemap.color.a;
        LeanTween.value(gameObject, c =>
        {
            tilemap.color = c;
        }, startColor, layerColor, 1.1f).setEaseInQuad();
    }

    public void Hide()
    {
        LeanTween.value(gameObject, a => 
        {
            var c = tilemap.color;
            c.a = a;
            tilemap.color = c;
        }, 1, 0, 1f).setEaseInQuad();
    }

    public void SetBlock(Vector2Int blockPos, int blockId)
    {
        blocks[blockPosition.x, blockPosition.y] = blockId;
        UpdateTilemap(blockPos, blockId);
    }

    public void UpdateTilemap(Vector2 worldPosition, int blockId)
    {
        var tilePos = tilemap.WorldToCell(worldPosition);
        var tile = tilemap.GetTile<Tile>(tilePos);
        if (!tile)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            //tile.color = layerColor;
            tilemap.SetTile(tilePos, tile);
        }
        tile.sprite = ProceduralGenerator.Instance.blocksData[blockId].sprite;
        tilemap.RefreshTile(tilePos);
    }

    public void UpdateTilemap(Vector2Int blockPos, int blockId)
    {
        var tilePos = new Vector3Int(blockPos.x, blockPos.y);
        var tile = tilemap.GetTile<Tile>(tilePos);
        if (!tile)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            //tile.color = layerColor;
            tilemap.SetTile(tilePos, tile);
        }

        tile.sprite = ProceduralGenerator.Instance.blocksData[blockId].sprite;
        tilemap.RefreshTile(tilePos);
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

    private bool IsValidCoordinate(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < chunckSize && coord.y >= 0 && coord.y < chunckSize;
    }

    float dist;
    private void Update()
    {
        dist = Vector2.Distance(transform.position, player.position + (Vector3.one * WorldManager.Instance.chunkSize / 2));
        if (dist > WorldManager.Instance.chunkSize * (WorldManager.Instance.viewDistance + 3))
        {
            gameObject.SetActive(false);
        }
    }
}
