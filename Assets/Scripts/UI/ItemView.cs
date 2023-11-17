using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using static Inventory;

public class ItemView : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text txtCount;

    public void Init(Item item)
    {
        if (item == null)
        {
            txtCount.text = string.Empty;
            return;
        }

        icon.sprite = item.sprite;
        txtCount.text = $"x{item.count}";
    } 
}
