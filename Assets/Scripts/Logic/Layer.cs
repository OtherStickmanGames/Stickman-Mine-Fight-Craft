using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer
{
    public int CurLayer = 1;
    public Color colorBackSide = new(colorValue, colorValue, colorValue, 1);


    public static Layer Inst;

    const float colorValue = 0.8f;

    Dictionary<Vector2Int, Chunck> chuncks;

    public Layer(Dictionary<Vector2Int, Chunck> chuncks)
    {
        this.chuncks = chuncks;
        Inst = this;

        EventsHolder.onBtnPrevLayer.AddListener(PrevLayer_Clicked);
        EventsHolder.onBtnNextLayer.AddListener(NextLayer_Clicked);

        EventsHolder.onPlayerLayerChanged?.Invoke(CurLayer);
    }

    private void NextLayer_Clicked()
    {
        CurLayer++;
        EventsHolder.onPlayerLayerChanged?.Invoke(CurLayer);
        //Debug.Log(chuncks.Count);
    }

    private void PrevLayer_Clicked()
    {
        CurLayer--;
        EventsHolder.onPlayerLayerChanged?.Invoke(CurLayer);
        //Debug.Log(chuncks.Count);
    }
}
