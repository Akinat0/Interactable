
using System.Linq;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    public Pointer Pointer => pointer;

    public PointerRaycaster Raycaster => raycaster;

    public InputBuffer InputBuffer => inputBuffer;

    public InteractorAnimator InteractorAnimator => interactorAnimator;
    
    public ItemGhostController GhostController => ghostController;
    public ZoomedItemController ZoomedItemController => zoomedItemController;
    public Inventory Inventory => inventory;
    

    public Item HeldItem
    {
        get => heldItem;
        set
        {
            if(heldItem == value)
                return;
            
            if (heldItem != null && heldItem.TryGetAction(out MoveAction prevItemMoveAction)) 
                prevItemMoveAction.StopMoving(this);
            
            heldItem = value;

            if (heldItem != null && heldItem.TryGetAction(out MoveAction moveAction))
                moveAction.StartMoving(this);
        }
    }
    
    public Item ZoomedItem { get; set; }

    public Item SelectedItem { get; private set; }
    
    public bool IsZoomed => ZoomedItem != null;

    [SerializeField] Pointer pointer;
    [SerializeField] ZoomedItemController zoomedItemController;
    [SerializeField] PointerRaycaster raycaster;
    [SerializeField] InputBuffer inputBuffer;
    [SerializeField] InteractorAnimator interactorAnimator;
    [SerializeField] ItemGhostController ghostController;
    [SerializeField] Inventory inventory;

    Item heldItem;

    void Update()
    {
        UpdateSelectedItem();
        UpdateInput();
        ClearInputs();
    }

    void UpdateSelectedItem()
    {
        if (raycaster.IsEmpty)
        {
            SelectedItem = null;
            return;
        }
        
        bool IsNotZoomedItem(RaycastHit hit)
            => hit.collider != null && hit.collider.gameObject != ZoomedItem.gameObject;

        bool IsZoomedItem(RaycastHit hit)
            => !IsNotZoomedItem(hit);

        bool hasItemSelected;
        Item selectedItem;

        if (IsZoomed)
        {
            RaycastHit hit = HeldItem != null 
                ? raycaster.Buffer.FirstOrDefault(IsZoomedItem) 
                : raycaster.Buffer.FirstOrDefault(IsNotZoomedItem);

            if (hit.collider != null)
            {
                hit.collider.TryGetComponent(out selectedItem);
                hasItemSelected = true;
            }
            else
            {
                selectedItem = null;
                hasItemSelected = false;
            }
        }
        else
        {
            raycaster.Buffer[0].collider.TryGetComponent(out selectedItem);
            hasItemSelected = true;
        }

        SelectedItem = hasItemSelected ? selectedItem : null;
    }

    void UpdateInput()
    {
        if (ProcessInteractInput())
            return;
        
        if (ProcessInventoryInput())
            return;
        
        if (ProcessZoomedItemDrag())
            return;

        if (ProcessZoomOutInput())
            return;
        
        if (ProcessZoomInInput())
            return;
    }
    
    bool ProcessInteractInput()
    {
        if (!inputBuffer.IsInteract || raycaster.IsEmpty)
            return false;

        if (SelectedItem != null && SelectedItem.TryGetSuitableAction(this, Control.LeftMouseButton, out ItemAction action))
        {
            action.Process(this);
            return true;
        }
        
        //put held item. It's not allowed in zoom mode
        if (HeldItem != null && !IsZoomed)
        {
            Item puttedItem = HeldItem;
            HeldItem = null;
            
            puttedItem.Transform.parent = null;
            puttedItem.Transform.position = raycaster.Buffer[0].point;
            puttedItem.Transform.up = raycaster.Buffer[0].normal;

            puttedItem.EnablePhysics();
            puttedItem.EnableCollisions();
            puttedItem.gameObject.SetLayerRecursively(Layers.DefaultLayer);
            
            return true;
        }

        return false;
    }

    bool ProcessInventoryInput()
    {
        if (!inputBuffer.IsTakeInput)
            return false;

        if (HeldItem != null && HeldItem.TryGetSuitableAction(this, Control.RightMouseButton, out ItemAction heldItemAction))
        {
            heldItemAction.Process(this);
            return true;
        }

        if (SelectedItem != null &&
            SelectedItem.TryGetSuitableAction(this, Control.RightMouseButton, out ItemAction selectedItemAction))
        {
            selectedItemAction.Process(this);
            return true;
        }

        return false;
    }
    
    bool ProcessZoomedItemDrag()
    {
        if (!IsZoomed)
            return false;

        if (inputBuffer.IsDeselectInput)
        {
            ZoomedItemController.EndDrag();
            return true;
        }

        if (inputBuffer.IsInteract)
        {
            ZoomedItemController.BeginDrag();
            return true;
        }

        return false;
    }

    bool ProcessZoomOutInput()
    {
        if (!IsZoomed || !inputBuffer.IsZoomOut)
            return false;

        if (ZoomedItem.TryGetAction(out ZoomAction zoomAction))
            zoomAction.ZoomOut(this);

        return true;
    }
    
    bool ProcessZoomInInput()
    {
        if (IsZoomed || !inputBuffer.IsZoomIn)
            return false;

        if (HeldItem != null && HeldItem.TryGetSuitableAction(this, Control.Space, out ItemAction heldAction))
        {
            heldAction.Process(this);
            return true;
        }
        
        if (SelectedItem != null && SelectedItem.TryGetSuitableAction(this, Control.Space, out ItemAction selectedAction))
        {
            selectedAction.Process(this);
            return true;
        }

        return false;
    }

    void ClearInputs()
    {
        inputBuffer.IsInteract = false;
        inputBuffer.IsZoomIn = false;
        inputBuffer.IsZoomOut = false;
        inputBuffer.IsDeselectInput = false;
        inputBuffer.IsTakeInput = false;
    }
}
