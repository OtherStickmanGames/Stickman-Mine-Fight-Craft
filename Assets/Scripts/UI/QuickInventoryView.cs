using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuickInventoryView : MonoBehaviour
{
    [SerializeField] ItemView itemViewPrefab;
    [SerializeField] Transform parent;

    Inventory inventory;

    internal void Init(Player player)
    {
        inventory = player.GetComponent<Inventory>();

        inventory.updated += Inventory_Updated;

        UpdateView();
    }

    private void Inventory_Updated()
    {
        UpdateView();
    }

    void UpdateView()
    {
        Clear();

        for (int i = 0; i < inventory.quickSize; i++)
        {
            var data = inventory.quick.Count > i ? inventory.quick[i] : null;
            var view = Instantiate(itemViewPrefab, parent);
            view.Init(data);
        }
    }

    void Clear()
    {
        foreach (Transform item in parent)
        {
            Destroy(item.gameObject);
        }
    }
}
