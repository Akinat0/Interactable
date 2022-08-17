using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class UIInventoryCanvas : MonoBehaviour
{
    [SerializeField] RectTransform inventoryContainer;
    [SerializeField] Inventory inventory;
    [SerializeField] UIInventoryItem itemPrefab;

    ObjectPool<UIInventoryItem> itemsPool;
    readonly List<UIInventoryItem> activeItems = new (10);


    void Start()
    {
        itemsPool = new ObjectPool<UIInventoryItem>(CreateItem, GetItem, ReleaseItem, defaultCapacity: 3);
    }

    void OnDestroy()
    {
        itemsPool.Dispose();
    }

    void OnEnable()
    {
        inventory.Changed += Rebuild;
        Rebuild();
    }

    void OnDisable()
    {
        inventory.Changed -= Rebuild;
    }

    void Rebuild()
    {
        for (int i = activeItems.Count - 1; i >= 0; i--)
            itemsPool.Release(activeItems[i]);
        
        activeItems.Clear();
        
        for (int i = 0; i < inventory.ItemsCount; i++)
        {
            Item item = inventory.GetItem(i);
            
            if(item == null)
                continue;
            
            UIInventoryItem uiItem = itemsPool.Get();
            activeItems.Add(uiItem);
            
            uiItem.Name = item.Id;
            uiItem.SetIndex(i + 1);
        }
    }

    UIInventoryItem CreateItem()
    {
        return Instantiate(itemPrefab, inventoryContainer);
    }

    void GetItem(UIInventoryItem item)
    {
        item.gameObject.SetActive(true);
    }

    void ReleaseItem(UIInventoryItem item)
    {
        item.gameObject.SetActive(false);
    }
}
