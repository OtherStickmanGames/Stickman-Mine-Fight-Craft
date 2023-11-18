using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesPalette : MonoBehaviour
{
    [SerializeField] TileView tileViewPrefab;
    [SerializeField] Transform parent;
    [SerializeField] Sprite iconRemoveTile;

    Generator generator;

    public void Init()
    {
        generator = FindObjectOfType<Generator>();

        CreateTileViews();
    }

    void CreateTileViews()
    {
        var removeTileView = Instantiate(tileViewPrefab, parent);
        removeTileView.Init(-1, iconRemoveTile);
        removeTileView.onClick.AddListener(TileView_Clicked);

        for (int i = 0; i < generator.BlocksData.Length; i++)
        {
            var view = Instantiate(tileViewPrefab, parent);
            view.Init(i, generator.BlocksData[i].sprite);
            view.onClick.AddListener(TileView_Clicked);
        }
        
    }

    private void TileView_Clicked(int ID)
    {
        EventsHolder.onTileViewClick?.Invoke(ID);
    }
}
