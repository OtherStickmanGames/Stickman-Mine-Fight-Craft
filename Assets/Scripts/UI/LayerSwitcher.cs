using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] Button btnNextLayer;
    [SerializeField] Button btnPrevLayer;
    [SerializeField] TMP_Text txtCurLayer;

    public void Init()
    {
        btnNextLayer.onClick.AddListener(NextLayer_Clicked);
        btnPrevLayer.onClick.AddListener(PrevLayer_Clicked);

        EventsHolder.onPlayerLayerChanged.AddListener(PlayerLayer_Changed);
    }

    private void PlayerLayer_Changed(int value)
    {
        txtCurLayer.text = $"{value}";
    }

    private void PrevLayer_Clicked()
    {
        EventsHolder.onBtnPrevLayer?.Invoke();
    }

    private void NextLayer_Clicked()
    {
        EventsHolder.onBtnNextLayer?.Invoke();
    }

    
}
