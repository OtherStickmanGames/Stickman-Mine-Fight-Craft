using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static BuildingTemplateSaver;

public class Chunck : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap tilemapMiddle;
    [SerializeField] Tilemap tilemapBack;
    [SerializeField] new TilemapCollider2D collider;

    public List<Vector2Int> backTiles = new();
    public List<Vector2Int> backTilesBackLayer = new();

    public Tilemap Tilemap => tilemap;
    public Tilemap TilemapBack => tilemapBack;
    public Tilemap TilemapMiddle => tilemapMiddle;
    public TilemapCollider2D Collider => collider;

    Dictionary<Vector3Int, Interactable> interactableBlocks = new();

    public Tilemap CurTilemap
    {
        get 
        {
            if(Layer.Inst.CurLayer == 1)
            {
                return tilemap;
            }
            else
            {
                if (Miner.Instance.backMine)
                {
                    return tilemapBack;
                }
                else
                {
                    return tilemapMiddle;
                }
            }
        }
    }

    public void AddInteractBlock(Vector3Int tilePos, Interactable block)
    {
        //print((block as Block).Layer);
        interactableBlocks.Add(tilePos, block);
    }

    private void Update()
    {
        foreach (var kvPair in interactableBlocks)
        {
            var block = kvPair.Value;
            block.Interact();
        }
    }

    public Tilemap GetTilemap(int layer, bool backSide)
    {
        if (layer == 1)
        {
            return tilemap;
        }
        else
        {
            if (backSide)
            {
                return tilemapBack;
            }
            else
            {
                return tilemapMiddle;
            }
        }
    }

    public int Distance(Vector2 playerPos)
    {
        return (int)Vector2.Distance(playerPos, transform.position);
    }


}
