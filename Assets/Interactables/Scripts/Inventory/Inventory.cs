
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Inventory : MonoBehaviour, IEnumerable<Item>
{
    public event Action Changed; 

    readonly List<Item> items = new(10);

    public int ItemsCount => items.Count;

    public void AddItem(Item item)
    {
        items.Add(item);
        Changed?.Invoke();
    }
    
    public void AddItemFirst(Item item)
    {
        items.Insert(0, item);
        Changed?.Invoke();
    }

    [CanBeNull]
    public Item GetItem(int index) => index >= 0 && index < ItemsCount ? items[index] : null;

    public Item RemoveItem(int index)
    {
        Item item = GetItem(index);
        items.RemoveAt(index);
        Changed?.Invoke();
        return item;
    }

    public bool HasItem(int index) => index >= 0 && index < ItemsCount;

    public IEnumerator<Item> GetEnumerator() => items.GetEnumerator();
    

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}
