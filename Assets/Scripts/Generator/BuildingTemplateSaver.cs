using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class BuildingTemplateSaver : MonoBehaviour
{
    [SerializeField] Generator generator;

    bool saverModeOn = false;

    [SerializeField]
    List<BlockData> blocks = new();

    int curIdxBlock = 0;

    public class Ebososka
    {
        public List<BlockData> data;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadToEdit();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            saverModeOn = !saverModeOn;
            if (saverModeOn)
                print($"Ну, епта, запись пошла..");
            else
            {
                if (blocks.Count > 0)
                {
                    PrepareBlocksData();

                    var json = JsonUtility.ToJson(new Ebososka { data = blocks });
                    json = json.Replace(",", ",\n");
                    var path = $"{Application.dataPath}/Resources/Opto.json";
                    File.WriteAllText(path, json);
                    print($"запысал {blocks.Count} блока");

                }
            }


        }

        if (saverModeOn)
        {
            InputChooseBlock();

            if (Input.GetMouseButtonDown(0))
            {
                var layer = Layer.Inst.CurLayer;
                var backSide = Miner.Instance.backMine;
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                var chunck = generator.GetChunck(pos);

                var tilemap = chunck.Tilemap;

                if (Layer.Inst.CurLayer != 1 && Layer.Inst.CurLayer != 2)
                    Debug.Log("!!! НЕВЕРНЫЙ СЛОЙ !!! ЕБАНАШКА!!!");

                if (Layer.Inst.CurLayer == 2)
                {
                    tilemap = chunck.TilemapBack;
                }

                var tilePos = tilemap.WorldToCell(pos);

                Tile tile;
                if (curIdxBlock < 0)
                {
                    tile = null;
                }
                else
                {
                    tile = new()
                    {
                        sprite = generator.BlocksData[curIdxBlock].sprite,
                        color = backSide ? Layer.Inst.colorBackSide : Color.white,
                        colliderType = Tile.ColliderType.None,
                    };
                }

                tilemap.SetTile(tilePos, tile);
                tilemap.RefreshAllTiles();


                var worldPos = tilemap.CellToWorld(tilePos);
                var b = blocks.Find(b => b.pos == (Vector2)worldPos && b.layer == layer && b.isBack == backSide);
                //print("=============================");
                //foreach (var yi in blocks)
                //{
                //    print($"{yi.pos} ||| {yi.isBack} ||| {yi.layer}");
                //}
                //print("=============================");
                //print($"{b.pos} ||| {b.isBack} ||| {b.layer}");
                if (b != null)
                    blocks.Remove(b);

                if (tile)
                {
                    blocks.Add(new()
                    {
                        ID = curIdxBlock,
                        pos = worldPos,
                        layer = Layer.Inst.CurLayer,
                        isBack = backSide
                    });
                }
            }
        }
    }

    void LoadToEdit()
    {
        var json = Resources.Load("Opto")?.ToString();
        if (json == null)
            return;

        var obaniy = JsonUtility.FromJson<Ebososka>(json).data;

        Debug.Log(obaniy.Count);

        var startPos = (Vector2)generator.player.Hip.position + Vector2.down;//.playerStartPos;

        List<Vector2?> reservedTilesFirstLayer = new();// Тут хранится информация
        List<Vector2?> reservedTilesBackLayers = new();// о тайлах, которые не нужно
                                                       // перекрывать тайлом, который находится
                                                       // на той же позиции, но сзади
        foreach (var item in obaniy)
        {
            var setTile = true;
            var pos = startPos + item.pos;
            var chunck = generator.GetChunck(pos);
            var tilemap = item.layer == 1 ? chunck.Tilemap : chunck.TilemapBack;
            var cellPos = tilemap.WorldToCell(pos);
            var color = item.isBack ? Layer.Inst.colorBackSide : Color.white;
            var colliderType = Tile.ColliderType.None;

            if (item.layer == 1 && !item.isBack)
                colliderType = Tile.ColliderType.Sprite;

            //Debug.Log($"|{c}| = {curCheckPos} = {cellPos} = {chunck.gameObject}");
            Tile tile = new()
            {
                color = color,
                sprite = generator.BlocksData[item.ID].sprite,
                colliderType = colliderType,
            };

            if (item.layer == 1)
            {
                if (!item.isBack)
                {
                    reservedTilesFirstLayer.Add(pos);
                }
                else
                {
                    var t = reservedTilesFirstLayer.Find(t => t.Value == pos);
                    if (t != null)
                    {
                        setTile = false;
                    }
                }
            }

            if (item.layer == 2)
            {
                if (!item.isBack)
                {
                    reservedTilesBackLayers.Add(pos);
                }
                else
                {
                    var t = reservedTilesBackLayers.Find(t => t.Value == pos);
                    if (t != null)
                    {
                        setTile = false;
                    }
                }
            }

            if (setTile)
            {
                tilemap.SetTile(cellPos, tile);
                tilemap.RefreshTile(cellPos);
            }

            item.pos = tilemap.CellToWorld(cellPos);
        }

        blocks = obaniy;
    }

    void PrepareBlocksData()
    {
        float minY = float.MaxValue;
        float minX = float.MaxValue;

        foreach (var item in blocks)
        {
            if (minX > item.pos.x)
                minX = item.pos.x;

            if (minY > item.pos.y)
                minY = item.pos.y;
        }

        foreach (var b in blocks)
        {
            b.pos -= new Vector2(minX, minY);
        }

        
    }

    void InputChooseBlock()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            curIdxBlock = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            curIdxBlock = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            curIdxBlock = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            curIdxBlock = 3;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            curIdxBlock = 4;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            curIdxBlock = 5;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            curIdxBlock = 6;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            curIdxBlock = -1;
        }
    }

    [System.Serializable]
    public class BlockData
    {
        public Vector2 pos;
        public int ID;
        public bool isBack;
        public int layer;
    }
}
