using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public int mainSize = 8;
    public int quickSize = 4;
    public List<Item> main = new();
    public List<Item> quick = new();

    public Action updated;

    Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    public void TakeItem(Item item)
    {
        var stacks = quick.FindAll(i => i.id == item.id);
        var stack = stacks.Find(s => s.count < s.stackValue);
        if(stack != null)
        {
            print($"Меня снимаешь? {item.count}");
            stack.count += item.count;
            print(stack.count);
            updated?.Invoke();
        }
        else if(quick.Count < quickSize)
        {
            quick.Add(item);

            updated?.Invoke();
        }
    }

    public class Item
    {
        public int id;
        public int count = 1;
        public int stackValue = 8;
        public Sprite sprite;
    }
}


