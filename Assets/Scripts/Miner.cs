using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class Miner : MonoBehaviour
{
    [SerializeField] BlockSpawned blockSpawnedPrefab;
    [SerializeField] Sprite[] destructionsSprites;
    [SerializeField] SpriteRenderer animDestructTile;

    Generator generator;

    public static Miner Instance;

    public bool backMine;

    private void Awake()
    {
        Instance = this;

        generator = FindObjectOfType<Generator>();

        EventsHolder.onTileMined.AddListener(Tile_Mined);
    }

    private void Tile_Mined(Mineable mineable)
    {
        var block = Instantiate(blockSpawnedPrefab, mineable.blockGlobalPos, Quaternion.identity);
        block.Init(mineable.tile.sprite);
        animDestructTile.sprite = null;
    }

    public Mineable GetTile(Vector2 pos, Vector2 dir, int distance)
    {
        if (dir.y < 0)
            pos += Vector2.down * 0.5f;
        if (dir.x > 0)
            pos += Vector2.right * 0.5f;

        var opta = GetTilesByDirection(pos, dir, distance);
        opta = opta.FindAll(t => t.tile.colliderType != Tile.ColliderType.None);

        if (backMine)
        {
            opta = GetTilesByWithoutRaycast(pos, dir, distance);
        }

        
        // ===!!! Для проверки тайлов которые попали в рейкаст !!!===
        //foreach (var item in opta)
        //{
        //    item.tile.color = Color.blue;
        //    item.tilemap.RefreshTile(item.blockTilePos);
        //}
        // !!!=====================================================!!!

        opta = opta.OrderBy(t => t.distance).ToList();

        if (opta.Count > 0)
            return opta.First();


        //for (int i = 1; i < distance; i++)
        //{
        //    Vector2 origin = pos + (dir * i);

        //    var hits = Physics2D.CircleCastAll(origin, 0.8f, Vector2.zero);

        //    hits = hits.ToList().FindAll(h => h.collider.GetComponent<Tilemap>()).ToArray();

        //    var hit = GetNearestTilemap(pos, hits);

        //    if (hit)
        //    {
        //        List<Tile> tiles = new();

        //        var tilemap = hit.collider.GetComponent<Tilemap>();
        //        var tile = GetNearestTileByHit(tilemap, pos, hit.point, out var tilePos);
        //        tiles.Add(tile);

        //        //var tileBase = tilemap.GetTile(tilePos);
        //        if (tile)
        //        {
        //            //var tile = tileBase as Tile;
        //            tile.color = Color.green;
        //            tilemap.RefreshTile(tilePos);

        //            var blockGlobalPos = tilemap.transform.position + tilePos + new Vector3(0.5f, 0.5f);

        //            return new()
        //            {
        //                blockGlobalPos = blockGlobalPos,
        //                tile = tile,
        //                tilemap = tilemap,
        //                blockTilePos = tilePos,
        //            };
        //        }
        //    }
        //}

        return null;
    }

    List<Mineable> GetTilesByWithoutRaycast(Vector2 playerPos, Vector2 dir, int distance)
    {
        List<Mineable> tilesData = new();

        //print($"{dir} === {playerPos}");
        var horizontalMine = Mathf.Abs(dir.x) > 0;
        int minX = horizontalMine ? 0 : -2;
        int maxX = horizontalMine ? distance : 3;
        int minY = horizontalMine ? -1 : dir.y > 0 ? 0 : 1;
        int maxY = horizontalMine ? 1 : distance; 

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                var offset = dir;
                offset.x += x * Mathf.Sign(dir.x);
                offset.y += y * Mathf.Sign(dir.y);
                playerPos.y = Mathf.Round(playerPos.y);
                var pos = playerPos + offset;

                if (Vector2.Distance(pos, playerPos) > distance)
                    continue;

                var chunck = generator.GetChunck(pos);
                if (chunck)
                {
                    var tilemap = Layer.Inst.CurLayer == 1 ? chunck.Tilemap : chunck.TilemapBack;
                    var tilePos = tilemap.WorldToCell(pos);
                    var tile = tilemap.GetTile<Tile>(tilePos);
                    if (tile)
                    {
                        var tileGlobalPos = tilemap.transform.position + tilePos + new Vector3(0.5f, 0.5f);
                        Mineable m = new()
                        {
                            tile = tile,
                            blockGlobalPos = tileGlobalPos,
                            blockTilePos = tilePos,
                            tilemap = tilemap,
                            distance = Vector2.Distance(playerPos, pos)
                        };
                        tilesData.Add(m);
                    }
                }
            }
        }

        return tilesData;
    }

    List<Mineable> GetTilesByDirection(Vector2 playerPos, Vector2 dir, int distance)
    {
        List<Mineable> tilesData = new();
        List<Tile> tiles = new();

        for (int i = 1; i < distance; i++)
        {
            int startY = dir.x > 0 || dir.x < 0 ? -1 : -i;
            int maxY = dir.x > 0 || dir.x < 0 ? 1 : i;
            startY = dir.y > 0 ? 0 : startY;
            for (int x = -i; x < i; x++)
            {
                for (int y = startY; y < maxY; y++)
                {
                    var offset = new Vector2(x, y);
                    var origin = playerPos + (dir * i) + offset;
                    var hits = Physics2D.CircleCastAll(origin, 0.3f, Vector2.zero);
                    var hit = hits.ToList().Find(h => h.collider.GetComponent<Tilemap>());
                    var tilemap = hit ? hit.collider.GetComponent<Tilemap>() : null;
                    if (tilemap)
                    {
                        var tilePos = tilemap.WorldToCell(origin);
                        var tile = tilemap.GetTile<Tile>(tilePos);
                        if (tile)
                        {
                            if (!tiles.Contains(tile))
                            {
                                tiles.Add(tile);
                                var tileGlobalPos = tilemap.transform.position + tilePos + new Vector3(0.5f, 0.5f);
                                tilesData.Add(new Mineable
                                {
                                    tile = tile,
                                    tilemap = tilemap,
                                    blockGlobalPos = tileGlobalPos,
                                    blockTilePos = tilePos,
                                    distance = Vector2.Distance(playerPos, tileGlobalPos),
                                });
                            }
                        }
                    }
                }
            }            
        }

        return tilesData;
    }

    Tile GetNearestTileByHit(Tilemap tilemap, Vector2 playerPos, Vector2 hitPos, out Vector3Int tilePos)
    {
        Tile tile = null;
        tilePos = default;

        var dir = (hitPos - playerPos).normalized * 0.5f;

        var offset = dir;

        while (tile == null)
        {
            tilePos = tilemap.WorldToCell(playerPos + offset);

            var tileBase = tilemap.GetTile(tilePos);
            if(tileBase != null)
            {
                tile = tileBase as Tile;
                break;
            }
            else
            {
                offset += dir;
            }

            if(offset.magnitude > 10)
            {
                print("ПИЗДЕЦ");
                break;
            }
        }

        return tile;
    }

    RaycastHit2D GetNearestTilemap(Vector2 playerPos, RaycastHit2D[] hits)
    {
        RaycastHit2D hit = default;

        float minDistance = 888;
        foreach (var item in hits)
        {
            var distance = Vector2.Distance(playerPos, item.point);
            if (distance < minDistance)
            {
                minDistance = distance;
                hit = item;
            }
        }

        return hit;
    }

    public Vector2 GetMineEffectPos(Vector3 pos, Vector3 dir)
    {
        var hits = Physics2D.RaycastAll(pos, dir, 5);
        return hits.ToList().Find(h => h.collider.GetComponent<Tilemap>()).point;
    }

    public void Destruction(Mineable mineable, float currentMineTime, float maxMineTime)
    {
        float step = maxMineTime / destructionsSprites.Length;

        int idx = (int)(currentMineTime / step);

        animDestructTile.sprite = destructionsSprites[idx];

        animDestructTile.transform.position = mineable.blockGlobalPos;
    }
}

public class Mineable
{
    public Tile tile;
    public Tilemap tilemap;
    public Vector3 blockGlobalPos;
    public Vector3Int blockTilePos;
    public float distance;
}


