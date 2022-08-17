
using UnityEngine;

[DisallowMultipleComponent]
public class ZoomAction : ItemAction
{
    public override Control Control => Control.Space;

    bool wasHeldItem;

    public override void Process(Interactor interactor)
    {
        ZoomIn(interactor);
    }

    public override string GetDescription(Interactor interactor)
        => interactor.ZoomedItem != Item ? "Zoom" : null;

    public void ZoomIn(Interactor interactor)
    {
        wasHeldItem = interactor.HeldItem == Item;

        if (wasHeldItem)
            interactor.HeldItem = null;

        interactor.InteractorAnimator.PlayZoomInItemAnimation(Item);
        
        //Disable physics for child items
        foreach (Item item in Item.GetComponentsInChildren<Item>())
            item.DisablePhysics();
        
        Item.gameObject.SetLayerRecursively(Layers.OverlayLayer);
        interactor.Raycaster.RaycastLayerMask = Layers.OverlayLayerMask;

        interactor.Pointer.IsLocked = false;
        interactor.ZoomedItemController.SetZoomedItem(Item);
        interactor.InputBuffer.SetZoomedItemActionsMap();
        interactor.ZoomedItem = Item;
        TimeManager.Pause();
    }

    public void ZoomOut(Interactor interactor)
    {
        TimeManager.Unpause();
        
        if (wasHeldItem)
            interactor.HeldItem = Item;
        
        interactor.InteractorAnimator.PlayZoomOutItemAnimation(() =>
        {
            Item.gameObject.SetLayerRecursively(Layers.DefaultLayer);
            interactor.Raycaster.RaycastLayerMask = Layers.DefaultRaycastLayersMask;

            if (wasHeldItem) return;
            
            //should not enable physics for held item
            foreach (Item item in Item.GetComponentsInChildren<Item>())
                item.EnablePhysics();
        });
        
        interactor.Pointer.IsLocked = true;
        interactor.ZoomedItemController.SetZoomedItem(null);
        interactor.InputBuffer.SetPlayerActionMap();
        interactor.ZoomedItem = null;
    }
}
