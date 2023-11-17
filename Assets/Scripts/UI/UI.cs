using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [SerializeField] TMP_Text txtBackOrFront;
    [SerializeField] Button btnMineMode;
    [SerializeField] Color frontMineColor;
    [SerializeField] Color backMineColor;

    [Space(8)]

    [SerializeField] LayerSwitcher layerSwitcher;

    public HUD hud;
    public PlayerInput playerInput;

    private void Start()
    {
        EventsHolder.playerSpawnedMine.AddListener(Player_Spawned);
        EventsHolder.onMineModeSwitch.AddListener(MineMode_Switched);

        btnMineMode.onClick.AddListener(MineMode_Clicked);

        MineMode_Switched(false);

        layerSwitcher.Init();
    }

    private void MineMode_Clicked()
    {
        EventsHolder.onLeftControl?.Invoke();
    }

    private void MineMode_Switched(bool value)
    {
        var text = value ? "≈бем задник" : "’у€рь передник";
        txtBackOrFront.text = text;
        txtBackOrFront.color = value ? backMineColor : frontMineColor;
    }

    private void Player_Spawned(Player player)
    {
        hud.Init(player);
        playerInput.Init(player);
    }
}
