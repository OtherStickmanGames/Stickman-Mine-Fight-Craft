using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunck : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap tilemapBack;
    [SerializeField] new TilemapCollider2D collider;

    public List<Vector2Int> backTiles = new();
    public List<Vector2Int> backTilesBackLayer = new();

    public Tilemap Tilemap => tilemap;
    public Tilemap TilemapBack => tilemapBack;
    public TilemapCollider2D Collider => collider;

    public int Distance(Vector2 playerPos)
    {
        return (int)Vector2.Distance(playerPos, transform.position);
    }


}
