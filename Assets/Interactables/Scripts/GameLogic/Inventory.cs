
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Inventory : MonoBehaviour, IEnumerable<Item>
{
    [SerializeField] InputBuffer inputBuffer;
    [SerializeField] Interactor interactor;

    public event Action Changed;

    public int ItemsCount => items.Count;
    
    readonly List<Item> items = new(10);

    void Update()
    {
        if (!inputBuffer.IsInventoryInput) 
            return;

        Item item = GetItem(inputBuffer.InventoryInputValue - 1);
        
        if(item == null)
            return;

        GrabAction grabAction = item.GetAction<GrabAction>();
        
        grabAction.TakeFromInventory(interactor);
        
        //clear input
        inputBuffer.IsInventoryInput = false;
    }
    
    public int IndexOf(Item item)
    {
        return items.IndexOf(item);
    }

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

    public IEnumerator<Item> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}
