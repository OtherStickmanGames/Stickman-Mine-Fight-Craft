using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.Events;

public class TileView : MonoBehaviour
{
    [SerializeField] Button button;
    

    public UnityEvent<int> onClick;

    int ID;

    public void Init(int ID, Sprite sprite)
    {
        this.ID = ID;

        button.image.sprite = sprite;

        button.onClick.AddListener(Tile_Clicked);
    }

    private void Tile_Clicked()
    {
        onClick?.Invoke(ID);   
    }
}
