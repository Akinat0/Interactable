
using UnityEngine;
using UnityEngine.Events;

public class ItemReceiver : Item
{
    [SerializeField] Transform itemPivot;
    [SerializeField] string targetItemName;
    [SerializeField] bool canReceiveItem = true;
    [SerializeField] UnityEvent<Item> onItemReceive;
    public Transform ItemPivot => itemPivot;
    public bool CanReceiveItem
    {
        get => canReceiveItem;
        set => canReceiveItem = value;
    }
    
    public bool CanReceive(Item item) => canReceiveItem && targetItemName == item.Name;

    public void ReceiveItem(Item item)
    {
        onItemReceive.Invoke(item);
    }
    
}
