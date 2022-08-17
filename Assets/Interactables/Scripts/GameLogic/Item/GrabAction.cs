using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MoveAction))]
public class GrabAction : ItemAction
{
    public override Control Control => Control.RightMouseButton;
    public override void Process(Interactor interactor)
    {
        if (interactor.HeldItem == Item)
            PutHeldIntoInventory(interactor, true);
        else
            PutIntoInventory(interactor);
    }
    
    public override string GetDescription(Interactor interactor) => "Grab";

    void PutHeldIntoInventory(Interactor interactor, bool insertFirst)
    {
        if(interactor.HeldItem == null)
            return;
        
        if(insertFirst)
            interactor.Inventory.AddItemFirst(interactor.HeldItem);
        else
            interactor.Inventory.AddItem(interactor.HeldItem);
            
        interactor.HeldItem.gameObject.SetActive(false);
        interactor.HeldItem = null;
    }

    void PutIntoInventory(Interactor interactor)
    {
        //by the main idea of interaction between interactor and inventory
        //firstly, we should put item into hands, and then add it to the inventory, 
        //it allows all systems to cache data correctly
        Item heldItem = interactor.HeldItem;
        interactor.HeldItem = Item;
        interactor.HeldItem = heldItem;
        
        Item.gameObject.SetActive(false);
        interactor.Inventory.AddItemFirst(Item);
    }
    
    public void TakeFromInventory(Interactor interactor)
    {
        int index = interactor.Inventory.IndexOf(Item);

        if(index < 0)
            return;
        
        PutHeldIntoInventory(interactor, false);
        
        interactor.Inventory.RemoveItem(index);
        interactor.HeldItem = Item;
        interactor.HeldItem.gameObject.SetActive(true);
    }

}
