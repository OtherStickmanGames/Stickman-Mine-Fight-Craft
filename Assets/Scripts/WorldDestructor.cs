using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;
using System;

public class WorldDestructor : MonoBehaviour
{
    [SerializeField] BlockSpawned blockPrefab;
    [SerializeField] FPSCounter fpsCounter;

    public static WorldDestructor Instance;

    Generator generator;

    List<BlockSpawned> blocks = new();
    float perfomanceToDoTimer;

    private void Awake()
    {
        Instance = this;

        generator = GetComponent<Generator>();

        EventsHolder.onBodyPartCollicion.AddListener(Noob_Collisioned);
    }

    private void Update()
    {
        CheckPerfomance();
    }

    private void Noob_Collisioned(CollisionHandler part, Collision2D coll)
    {
        var tilemap  = coll.gameObject.GetComponent<Tilemap>();
        int countBlocks = 0;
        if (tilemap)
        {
            var worldPos = coll.GetContact(0).point;

            var contactPos = tilemap.WorldToCell(worldPos);
            Vector2 center = new(contactPos.x, contactPos.y);

            int radius = (int)((coll.relativeVelocity.magnitude / Time.timeScale) * 0.08f);

            int maxRadius = 30;// part.Player.Team == Team.Hostile ? 18 : 30;
            radius = Mathf.Clamp(radius, 1, maxRadius);
            
            if (radius > 1)
                AudioManager.Instance.RockStrinke(part.transform);

            for (int x = -radius; x < radius; x++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    var newPos = contactPos + new Vector3(x, y);
                    var distance = Vector2.Distance(center, newPos);
                    if(distance < radius)
                    {
                        int zaebalx = (int)newPos.x;
                        int zaebaly = (int)newPos.y;

                        Vector3 tilemapPos = tilemap.transform.position;
                        
                        if (zaebalx >= generator.chunckSize)
                        {
                            tilemapPos.x += generator.chunckSize * (zaebalx / generator.chunckSize);
                        }
                        if(zaebalx < 0)
                        {
                            tilemapPos.x -= generator.chunckSize + (generator.chunckSize * (Mathf.Abs(zaebalx+1) / generator.chunckSize));
                        }
                        if (zaebaly >= generator.chunckSize)
                        {
                            tilemapPos.y += generator.chunckSize * (zaebaly / generator.chunckSize);
                        }
                        if (zaebaly < 0)
                        {
                            tilemapPos.y -= generator.chunckSize + (generator.chunckSize * (Mathf.Abs(zaebaly+1) / generator.chunckSize));
                        }

                        //if(zaebalx >= generator.chunckSize)
                        //{
                        //    tilemapPos.x += generator.chunckSize;
                        //}
                        //if(zaebalx < 0)
                        //{
                        //    tilemapPos.x -= generator.chunckSize;
                        //}
                        //if(zaebaly >= generator.chunckSize)
                        //{
                        //    tilemapPos.y += generator.chunckSize;
                        //}
                        //if(zaebaly < 0)
                        //{
                        //    tilemapPos.y -= generator.chunckSize;
                        //}

                        if (tilemap.transform.position != tilemapPos)
                        {
                            try
                            {
                                var intPos = new Vector2Int(Mathf.RoundToInt(tilemapPos.x), Mathf.RoundToInt(tilemapPos.y));
                                //print(intPos);

                                var otherTilemap = generator.dict[intPos].Tilemap;
                                var offset = tilemap.transform.position - tilemapPos;
                                var otherPos = new Vector3Int(zaebalx + Mathf.RoundToInt(offset.x), zaebaly + Mathf.RoundToInt(offset.y));

                                var tile = otherTilemap.GetTile(otherPos) as Tile;
                                if (tile)
                                {
                                    if (countBlocks < 30)
                                    {
                                        var blockPos = new Vector3(otherPos.x + tilemapPos.x, otherPos.y + tilemapPos.y);
                                        SpawnBlock(tile, blockPos, coll, worldPos);
                                        countBlocks++;
                                    }

                                    otherTilemap.SetTile(otherPos, null);
                                    otherTilemap.RefreshTile(otherPos);
                                }
                            }
                            catch (Exception)
                            {
                            }
                            
                        }
                        else
                        {
                            var pos = new Vector3Int(zaebalx, zaebaly);
                            var tile = tilemap.GetTile(pos) as Tile;
                            if (tile)
                            {
                                var blockPos = new Vector3(pos.x + tilemapPos.x, pos.y + tilemapPos.y);
                                SpawnBlock(tile, blockPos, coll, worldPos);

                                tilemap.SetTile(pos, null);
                                tilemap.RefreshTile(pos);
                            }                            
                        }
                    }

                }
            }

        }
    }

    public void SpawnBlock(Tile tile, Vector3 pos, Collision2D coll, Vector3 worldPos)
    {
        if (fpsCounter.FPS < 10)
            return;

        var block = Instantiate(blockPrefab, pos, Quaternion.identity);
        block.Renderer.sprite = tile.sprite;
        //block.Init(generator.player, coll.relativeVelocity.normalized);
        var force = coll.relativeVelocity;
        block.Body.AddForceAtPosition(force, worldPos, ForceMode2D.Impulse);
        blocks.Add(block);
    }

    public void SpawnBlock(Tile tile, Vector3 pos, Vector2 dirEffect)
    {
        if (fpsCounter.FPS < 10)
            return;

        var block = Instantiate(blockPrefab, pos, Quaternion.identity);
        block.Renderer.sprite = tile.sprite;
        //block.Init(generator.player, dirEffect);
        blocks.Add(block);
    }

    public void Destruct(Tilemap tilemap, Vector2 worldPos, int radius)
    {
        int countBlocks = 0;
        if (tilemap)
        {
            var contactPos = tilemap.WorldToCell(worldPos);
            Vector2 center = new(contactPos.x, contactPos.y);

            radius = Mathf.Clamp(radius, 1, 30);
            for (int x = -radius; x < radius; x++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    var newPos = contactPos + new Vector3(x, y);
                    var distance = Vector2.Distance(center, newPos);
                    if (distance < radius)
                    {
                        int zaebalx = (int)newPos.x;
                        int zaebaly = (int)newPos.y;

                        Vector3 tilemapPos = tilemap.transform.position;
                        
                        if (zaebalx >= generator.chunckSize)
                        {
                            tilemapPos.x += generator.chunckSize * (zaebalx / generator.chunckSize);
                        }
                        if (zaebalx < 0)
                        {
                            tilemapPos.x -= generator.chunckSize + (generator.chunckSize * (Mathf.Abs(zaebalx + 1) / generator.chunckSize));
                        }
                        if (zaebaly >= generator.chunckSize)
                        {
                            tilemapPos.y += generator.chunckSize * (zaebaly / generator.chunckSize);
                        }
                        if (zaebaly < 0)
                        {
                            tilemapPos.y -= generator.chunckSize + (generator.chunckSize * (Mathf.Abs(zaebaly + 1) / generator.chunckSize));
                        }

                        if (tilemap.transform.position != tilemapPos)
                        {
                            try
                            {
                                var intPos = new Vector2Int((int)tilemapPos.x, (int)tilemapPos.y);
                                //print(intPos);

                                var otherTilemap = generator.dict[intPos].Tilemap;
                                var offset = tilemap.transform.position - tilemapPos;
                                var otherPos = new Vector3Int(zaebalx + (int)offset.x, zaebaly + (int)offset.y);

                                var tile = otherTilemap.GetTile(otherPos) as Tile;
                                if (tile)
                                {
                                    if (countBlocks < 30)
                                    {
                                        var blockPos = new Vector3(otherPos.x + tilemapPos.x, otherPos.y + tilemapPos.y);
                                        Vector2 dir = worldPos - (worldPos + new Vector2(x, y));
                                        SpawnBlock(tile, blockPos, dir.normalized);
                                        countBlocks++;
                                    }

                                    otherTilemap.SetTile(otherPos, null);
                                    otherTilemap.RefreshTile(otherPos);
                                }
                            }
                            catch (Exception)
                            {
                            }

                        }
                        else
                        {
                            var pos = new Vector3Int(zaebalx, zaebaly);
                            var tile = tilemap.GetTile(pos) as Tile;
                            if (tile)
                            {
                                var blockPos = new Vector3(pos.x + tilemapPos.x, pos.y + tilemapPos.y);
                                Vector2 dir = worldPos - (worldPos + new Vector2(x, y));
                                SpawnBlock(tile, blockPos, dir.normalized);

                                tilemap.SetTile(pos, null);
                                tilemap.RefreshTile(pos);
                            }
                        }
                    }

                }
            }
        }
    }

    void CheckPerfomance()
    {
        perfomanceToDoTimer += Time.deltaTime;

        if (fpsCounter.FPS < 18 && perfomanceToDoTimer > 0.1f)
        {
            blocks.RemoveAll(b => b == null);
            //print($"шо то тяжко +++++++++   {blocks.Count}");

            blocks = blocks.OrderBy(b => b.Distance).ToList();

            if(blocks.Count > 0)
            {
                //print($"{blocks.Last().Distance} ============== {blocks.First().Distance} ====================");
                Destroy(blocks.Last());

                //perfomanceToDoTimer = 0;
            }

            //var maxDistance = 0f;
            //Block farBlock = null;

            //foreach (var block in blocks)
            //{
            //    if(block && block.Distance > maxDistance)
            //    {
            //        farBlock = block;
            //        maxDistance = block.Distance;
            //    }
            //}

            //if (farBlock)
            //{
            //    perfomanceToDoTimer = 0;

            //    Destroy(farBlock.gameObject);

            //    //blocks.RemoveAll(b => b == null);

            //    print("==========================");
            //}
        }
    }
}
