using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
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
    [SerializeField] TilesPalette tilesPalette;

    public HUD hud;
    public PlayerInput playerInput;

    private void Start()
    {
        EventsHolder.playerSpawnedMine.AddListener(Player_Spawned);
        EventsHolder.onMineModeSwitch.AddListener(MineMode_Switched);
        EventsHolder.onBuildEditorMode.AddListener(BuildEditorState_Changed);

        //btnMineMode.onClick.AddListener(MineMode_Clicked);

        //MineMode_Switched(false);

        layerSwitcher.Init();
        //tilesPalette.Init();

        //BuildEditorState_Changed(false);
    }

    private void BuildEditorState_Changed(bool enabled)
    {
        tilesPalette.gameObject.SetActive(enabled);
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

    public static bool IsHited()
    {
        var eventSystem = EventSystem.current;
        var result = new List<RaycastResult>();
        var pointer = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };
        eventSystem.RaycastAll(pointer, result);

        var uiHit = result.Find(hit => hit.gameObject.layer == LayerMask.NameToLayer("UI"));

        return uiHit.gameObject;
    }
}
