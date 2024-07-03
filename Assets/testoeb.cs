using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class testoeb : MonoBehaviour
{
    public Sprite sprite;
    public Vector2Int Position { get; private set; }
    private bool[,] blocks;
    private const int chunckSize = 16;

    Tilemap tilemap;

    private void Start()
    {
        Initialize(Vector2Int.zero);
    }

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
}
