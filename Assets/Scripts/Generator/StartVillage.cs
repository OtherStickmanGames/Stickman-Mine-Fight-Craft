using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StartVillage
{
    Generator generator;
    public int yStartOffset = 0;

    public StartVillage(Generator generator)
    {
        this.generator = generator;

        var startPos = generator.playerStartPos;
        var blockID = generator.GetBlockID(startPos.x, startPos.y);

        var chunckSize = generator.chunckSize;

        FindPlacePositions();
        

        for (int x = startPos.x - 30; x < startPos.x + 8; x++)
        {
            var curCheckPos = new Vector2(x, startPos.y + yStartOffset);

            var chunck = generator.GetChunck(curCheckPos);
            
            var cellPos = chunck.Tilemap.WorldToCell(curCheckPos);

            //Debug.Log($"|{c}| = {curCheckPos} = {cellPos} = {chunck.gameObject}");
            var tile = chunck.Tilemap.GetTile<Tile>(cellPos);
            if (tile)
            {
                tile.color = Color.cyan;
                chunck.Tilemap.RefreshTile(cellPos);
            }
            else
            {
                tile = new() { color = Color.white, sprite = generator.BlocksData[3].sprite };
                chunck.Tilemap.SetTile(cellPos, tile);
                chunck.Tilemap.RefreshTile(cellPos);
                chunck.TilemapBack.SetTile(cellPos, tile);
                chunck.TilemapBack.RefreshTile(cellPos);
            }

            //Debug.Log($"{curCheckPos} = {tile} = {startPos == curCheckPos} -+-+ {chuncks.Count}");
        }

        Debug.Log($"Убаный шашлык {yStartOffset}");

        foreach (var item in generator.startVillageData.buildings)
        {
            CreateBuilding(item.tilesData.name, item.leftPos);
        }
        
    }

    void FindPlacePositions()
    {
        var startPos = generator.playerStartPos;

        for (int x = startPos.x - 30; x < startPos.x + 8; x++)
        {
            var curCheckPos = new Vector2(x, startPos.y + yStartOffset + 1);
            var chunck = generator.GetChunck(curCheckPos);
            
            var cellPos = chunck.Tilemap.WorldToCell(curCheckPos);
            var tile = chunck.Tilemap.GetTile<Tile>(cellPos);
            if (tile)
            {
                yStartOffset++;
                FindPlacePositions();
                return;
            }
        }
    }

    void CreateBuilding(string buildName, int posX)
    {
        var json = Resources.Load(buildName)?.ToString();
        if (json == null)
            return;

        var obaniy = JsonUtility.FromJson<BuildingTemplateSaver.Ebososka>(json).data;

        //Debug.Log(obaniy.Count);

        var startPos = generator.playerStartPos;
        startPos.y += yStartOffset + 1;

        List<Vector2?> reservedTilesFirstLayer = new();// Тут хранится информация
        List<Vector2?> reservedTilesBackLayers = new();// о тайлах, которые не нужно
                                                      // перекрывать тайлом, который находится
                                                      // на той же позиции, но сзади


        foreach (var item in obaniy)
        {
            var setTile = true;
            var pos = startPos - new Vector2(posX, 0) + item.pos;
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

            if(item.layer == 1)
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
        }
    }
}
