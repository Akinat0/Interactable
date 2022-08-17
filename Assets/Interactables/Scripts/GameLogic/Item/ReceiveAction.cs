
using UnityEngine;
using UnityEngine.Events;

public class ReceiveAction : ItemAction
{
    [SerializeField] Transform itemPivot;
    [SerializeField] string targetItemId;
    [SerializeField] UnityEvent<Item> onItemReceive;

    public Transform ItemPivot => itemPivot;
    
    public override Control Control => Control.LeftMouseButton;

    public override bool CanProcess(Interactor interactor, Control control)
    {
        return base.CanProcess(interactor, control) && CanProcessItem(interactor.HeldItem);
    }

    public bool CanProcessItem(Item item)
    {
         return IsEnabled && item != null && item.Id == targetItemId;
    }
    
    public override void Process(Interactor interactor)
    {
        Item item = interactor.HeldItem;
        
        item.Transform.parent = itemPivot;
        item.Transform.localPosition = Vector3.zero;
        item.Transform.localRotation = Quaternion.identity;
        
        if (!interactor.IsZoomed)
            item.EnablePhysics();
        
        item.EnableCollisions();
        item.gameObject.SetLayerRecursively(gameObject.layer);
        
        onItemReceive?.Invoke(item);

        interactor.HeldItem = null;
    }

    public override string GetDescription(Interactor interactor)
    {
        return null;
    }
}
