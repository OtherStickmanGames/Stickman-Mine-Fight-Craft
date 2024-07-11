using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using Architecture;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] Button btnNextLayer;
    [SerializeField] Button btnPrevLayer;
    [SerializeField] TMP_Text txtCurLayer;
    [SerializeField] Color notAvailableLayer;
    [SerializeField] Color normalColor;


    public void Init()
    {
        btnNextLayer.onClick.AddListener(NextLayer_Clicked);
        btnPrevLayer.onClick.AddListener(PrevLayer_Clicked);

        EventsHolder.onPlayerLayerChanged.AddListener(PlayerLayer_Changed);
        EventsHolder.onAvailableSwitchLayer.AddListener(AvailableSwitchLayer_Invoked);
    }

    Color curColor;
    private void AvailableSwitchLayer_Invoked(bool value, LayerSwitchDir dir)
    {
        curColor = value ? normalColor : notAvailableLayer;
        switch (dir)
        {
            case LayerSwitchDir.Next:
                btnNextLayer.image.color = curColor;
                break;

            case LayerSwitchDir.Prev:
                btnPrevLayer.image.color = curColor;
                break;
            
        }
    }

    private void PlayerLayer_Changed(int value)
    {
        txtCurLayer.text = $"{value + 1}";
        btnPrevLayer.interactable = value != 0;
        btnNextLayer.interactable = value != WorldManager.Instance.numLayers - 1;
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
