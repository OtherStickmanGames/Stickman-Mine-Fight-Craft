using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] QuickInventoryView quickInventory;

    Player player;

    public void Init(Player player)
    {
        this.player = player;

        quickInventory.Init(player);
    }
}
