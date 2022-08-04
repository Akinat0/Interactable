using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractorController : MonoBehaviour
{
    Camera Camera { get; set; }
    public Item SelectedItem { private set; get; }
    
    
    MovableItem heldItem;
    public MovableItem HeldItem
    {
        private set
        {
            if (heldItem != null)
            {
                heldItem.ReleaseGhost();
                itemGhost = null;
            }

            heldItem = value;

            if (heldItem != null)
            {
                itemGhost = heldItem.GetGhost();
                heldItem.gameObject.SetLayerRecursively(characterLayer);
            }
            
        }
        get => heldItem;
    }

    public Item ZoomedItem { private set; get; }
    public bool IsZoomed => ZoomedItem != null;
    
    public Vector2 PointerPosition { get; private set; }
    
    [SerializeField] float interactionDistance = 10;
    [SerializeField] float takeDuration = 0.3f;
    [SerializeField] Transform heldItemPivot;
    [SerializeField] Transform zoomedItemPivot;
    [SerializeField] UIInteractableCanvas interactableCanvas;
    [SerializeField] InputBuffer inputBuffer;
    [SerializeField] Inventory inventory;

    readonly IComparer<RaycastHit> HitComparer = 
        Comparer<RaycastHit>.Create((lhs, rhs) => lhs.distance.CompareTo(rhs.distance));

    int overlayLayer;
    int defaultLayer;
    int characterLayer;
    LayerMask overlayLayerMask;
    bool isHitBufferEmpty;
    RaycastHit[] hitBuffer = new RaycastHit[5];
    ItemReceiver itemReceiver;
    Transform itemGhost;
    Transform zoomedItemCachedParent;
    Vector3 zoomedItemCachedPosition;
    Quaternion zoomedItemCachedRotation;
    Vector3 cachedHeldItemPivotPosition;
    
    Vector2? previousDragPosition;

    IEnumerator takeItemRoutine;
    IEnumerator zoomInItemRoutine;
    IEnumerator zoomOutItemRoutine;

    void Start()
    {
        Camera = Camera.main;
        overlayLayer = LayerMask.NameToLayer("Overlay");
        defaultLayer = LayerMask.NameToLayer("Default");
        characterLayer = LayerMask.NameToLayer("Character");
        overlayLayerMask = LayerMask.GetMask("Overlay");

        cachedHeldItemPivotPosition = heldItemPivot.localPosition;
    }

    void Update()
    {
        UpdatePointerPosition();
        UpdateRaycastBuffer();
        UpdateSelectedItem();
        UpdateItemReceiver();
        UpdateCanvas();
        UpdateInputs();
        UpdateGhost();
        UpdateZoomedItem();
        UpdatePhysics();
        UpdateHeldItemPosition();
        ClearInputBuffer();
    }

    void UpdatePhysics()
    {
        //Time scale equals to 0 while item zoomed, so we should sync transforms manually
        if(IsZoomed)
            Physics.SyncTransforms();
    }
    
    void UpdatePointerPosition()
    {
        PointerPosition = IsZoomed 
            ? InputBuffer.Instance.PtrPosition 
            : new Vector2(Screen.width, Screen.height) / 2;
    }
    
    void UpdateRaycastBuffer()
    {
        for (int i = 0; i < hitBuffer.Length; i++)
            hitBuffer[i] = new RaycastHit();

        Ray ray = Camera.ScreenPointToRay(PointerPosition);
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);
        int collisionsCount = Physics.RaycastNonAlloc(ray, hitBuffer, interactionDistance, IsZoomed ? overlayLayerMask : Physics.DefaultRaycastLayers);
        
        isHitBufferEmpty = collisionsCount <= 0;
        if(!isHitBufferEmpty)
            Array.Sort(hitBuffer, 0, Mathf.Min(hitBuffer.Length, collisionsCount), HitComparer);
    }

    void UpdateSelectedItem()
    {
        if (isHitBufferEmpty)
        {
            SelectedItem = null;
            return;
        }
        
        bool IsNotZoomedItem(RaycastHit hit)
            => hit.collider != null && hit.collider.gameObject != ZoomedItem.gameObject;

        bool hasItemSelected;
        Item selectedItem;

        if (IsZoomed)
        {
            RaycastHit hit = hitBuffer.FirstOrDefault(IsNotZoomedItem);

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
            hitBuffer[0].collider.TryGetComponent(out selectedItem);
            hasItemSelected = true;
        }

        SelectedItem = hasItemSelected ? selectedItem : null;
    }

    void UpdateCanvas()
    {
        string itemId = ZoomedItem != null 
            ? ZoomedItem.Name 
            : HeldItem != null 
                ? HeldItem.Name 
                : SelectedItem != null 
                    ? SelectedItem.Name 
                    : null;
        
        interactableCanvas.SetAim(HeldItem == null);
        interactableCanvas.SetFocused(SelectedItem != null);
        interactableCanvas.SetPointerPosition(PointerPosition);
        interactableCanvas.SetItemId(itemId);
        
        interactableCanvas.ClearActions();

        bool selectedIsMovable = SelectedItem != null && SelectedItem is MovableItem;
        bool selectedIsInteractable = SelectedItem != null && SelectedItem is InteractableItem;
        
        if(HeldItem != null && !isHitBufferEmpty)
            interactableCanvas.AddAction(Control.LeftMouseButton, "Place");
        else if (selectedIsMovable)
            interactableCanvas.AddAction(Control.LeftMouseButton, "Grab");
        else if (selectedIsInteractable)
            interactableCanvas.AddAction(Control.LeftMouseButton, "Interact");
        
        if(HeldItem != null || selectedIsMovable)
            interactableCanvas.AddAction(Control.RightMouseButton, "Take");

        if(HeldItem != null && HeldItem.IsZoomable || HeldItem == null && SelectedItem != null && SelectedItem.IsZoomable)
            interactableCanvas.AddAction(Control.Space, "Look");
    }

    void UpdateItemReceiver()
    {
        //we will update item receiver only if held item exists
        if (HeldItem == null || isHitBufferEmpty)
        {
            itemReceiver = null;
            return;
        }
        
        //in zoomed mode, zoomed item can't be selected, however it could be an item receiver, so check first hit additionally
        if (SelectedItem == null && !ZoomedItem)
        {
            itemReceiver = null;
            return;
        }

        if (IsZoomed && hitBuffer[0].collider.gameObject == ZoomedItem.gameObject)
        {
            itemReceiver = hitBuffer[0]
                .collider
                .GetComponents<ItemReceiver>()
                .FirstOrDefault(receiver => receiver.CanReceive(HeldItem));
            
            //zoomed item is item receiver
            if(itemReceiver != null)
                return;
        }
        
        if(SelectedItem != null)
            itemReceiver = SelectedItem.GetComponents<ItemReceiver>().FirstOrDefault(receiver => receiver.CanReceive(HeldItem));
    }

    void UpdateInputs()
    {
        if (ProcessDeselect())
            return;
        
        if (ProcessInventory())
            return;
        
        if (ProcessTakeIntoInventory())
            return;
        
        if (ProcessInteract())
            return;
        
        if (ProcessDrag())
            return;
        
        if (ProcessZoomOut())
            return;
        
        if (ProcessZoomIn())
            return;
    }

    bool ProcessDeselect()
    {
        if (!inputBuffer.IsDeselectInput || ZoomedItem == null)
            return false;
        
        previousDragPosition = null;
        return true;
    }

    bool ProcessInventory()
    {
        if (!inputBuffer.IsInventoryInput)
            return false;

        int itemIndex = inputBuffer.InventoryInputValue - 1;

        if (!inventory.HasItem(itemIndex))
            return false;

        TakeFromInventory(itemIndex);
        return true;
    }

    bool ProcessTakeIntoInventory()
    {
        if (!inputBuffer.IsTakeInput)
            return false;

        if (HeldItem != null)
        {
            HeldItem.gameObject.SetActive(false);
            inventory.AddItemFirst(HeldItem);
            HeldItem = null;
            return true;
        }

        if (SelectedItem is MovableItem movableItem)
        {
            movableItem.Transform.parent = heldItemPivot;
            movableItem.Transform.position = Vector3.zero;
            movableItem.gameObject.SetActive(false);
            movableItem.DisableCollisions();
            movableItem.DisablePhysics();
            inventory.AddItemFirst(movableItem);
            return true;
        }
        
        return false;
    }
    
    bool ProcessInteract()
    {
        if (!inputBuffer.IsInteract || isHitBufferEmpty)
            return false;
        
        if (HeldItem != null)
        {
            if (itemReceiver != null)
            {
                StopTakeItemRoutine();
                
                HeldItem.Transform.parent = itemReceiver.ItemPivot;
                HeldItem.Transform.localPosition = Vector3.zero;
                HeldItem.Transform.localRotation = Quaternion.identity;
                
                //don't enable physics in zoom mode
                if(!IsZoomed)
                    HeldItem.EnablePhysics();
                
                HeldItem.EnableCollisions();
                HeldItem.gameObject.SetLayerRecursively(itemReceiver.gameObject.layer);
                
                itemReceiver.ReceiveItem(HeldItem);
                
                HeldItem = null;
                return true;
            }
            
            //simply drop item (but it's not allowed in zoomed mode)
            if(!ZoomedItem)
            {
                StopTakeItemRoutine();
                
                HeldItem.Transform.parent = null;
                HeldItem.Transform.position = hitBuffer[0].point;
                HeldItem.Transform.up = hitBuffer[0].normal;

                HeldItem.EnablePhysics();
                HeldItem.EnableCollisions();
                HeldItem.gameObject.SetLayerRecursively(defaultLayer);
                
                HeldItem = null;
                return true;
            }
            
        }
        else if (SelectedItem != null)
        {
            if (SelectedItem.gameObject.TryGetComponent(out MovableItem movableItem))
            {
                SelectedItem = null;
                HeldItem = movableItem;
            
                HeldItem.DisablePhysics();
                HeldItem.DisableCollisions();

                StopTakeItemRoutine();
            
                StartCoroutine(takeItemRoutine = TakeItemRoutine());
                return true;
            }

            if (SelectedItem.gameObject.TryGetComponent(out InteractableItem interactableItem))
            {
                interactableItem.Interact();
                return true;
            }
        }

        return false;
    }

    bool ProcessDrag()
    {
        if (!inputBuffer.IsInteract || ZoomedItem == null) 
            return false;
        
        previousDragPosition = PointerPosition;

        return true;
    }

    bool ProcessZoomOut()
    {
        if (ZoomedItem == null || !inputBuffer.IsZoomOut)
            return false;

        StopZoomInRoutine();
        
        PostProcessing.Instance.SetCommonBackground(takeDuration);
        StartCoroutine(zoomOutItemRoutine = ZoomOutItemRoutine());
        return true;
    }
    
    bool ProcessZoomIn()
    {
        if (!inputBuffer.IsZoomIn || isHitBufferEmpty || zoomOutItemRoutine != null)
            return false;

        Item item;

        if (HeldItem != null && HeldItem.IsZoomable)
        {
            item = HeldItem;
            HeldItem = null;
        }
        else if (SelectedItem != null && SelectedItem.IsZoomable)
        {
            item = SelectedItem;
        }
        else return false;

        StopTakeItemRoutine();
        StopZoomInRoutine();
    
        ZoomedItem = item;

        PostProcessing.Instance.SetBlurBackground(takeDuration);
        StartCoroutine(zoomInItemRoutine = ZoomInItemRoutine());
        return true;
    }

    void UpdateGhost()
    {
        if(itemGhost == null)
            return;

        if (itemReceiver != null)
        {
            itemGhost.parent = itemReceiver.ItemPivot;
            itemGhost.localPosition = Vector3.zero;
            itemGhost.localRotation = Quaternion.identity;
            itemGhost.gameObject.SetActive(true);
            return;
        }

        if (isHitBufferEmpty || IsZoomed)
        {
            itemGhost.gameObject.SetActive(false);
            return;
        }

        itemGhost.parent = null;
        itemGhost.position = hitBuffer[0].point;
        itemGhost.up = hitBuffer[0].normal;
        itemGhost.gameObject.SetActive(true);
    }
    
    void UpdateZoomedItem()
    {
        if(ZoomedItem == null || !previousDragPosition.HasValue)
            return;
        
        float diff = PointerPosition.x - previousDragPosition.Value.x;
        previousDragPosition = PointerPosition;
        ZoomedItem.Transform.Rotate(0, -diff, 0);
    }

    void UpdateHeldItemPosition()
    {
        if (!IsZoomed || HeldItem == null)
        {
            heldItemPivot.localPosition = cachedHeldItemPivotPosition;
            return;
        }

        Vector3 position = heldItemPivot.position;
        Vector3 up = position + zoomedItemPivot.up;
        Vector3 right = position + zoomedItemPivot.right;
        
        Plane plane = new (position, up, right);

        Ray ray = Camera.ScreenPointToRay(PointerPosition);
        
        if(!plane.Raycast(ray, out float distance))
            return;

        heldItemPivot.position = ray.origin + ray.direction.normalized * distance;
    }
    
    void ClearInputBuffer()
    {
        inputBuffer.IsInteract = false;
        inputBuffer.IsZoomIn = false;
        inputBuffer.IsZoomOut = false;
        inputBuffer.IsDeselectInput = false;
        inputBuffer.IsTakeInput = false;
        inputBuffer.IsInventoryInput = false;
    }

    IEnumerator TakeItemRoutine()
    {
        HeldItem.Transform.parent = heldItemPivot;
        // yield return Coroutines.MoveLocalRealtime(takeDuration, HeldItem.Transform, Vector3.zero, Quaternion.identity);
        yield return Coroutines.MoveToTransformRealtime(takeDuration, HeldItem.Transform, heldItemPivot);
        takeItemRoutine = null;
    }
    
    IEnumerator ZoomInItemRoutine()
    {
        inputBuffer.SetZoomedItemActionsMap();
        
        zoomedItemCachedParent = ZoomedItem.Transform.parent;
        zoomedItemCachedPosition = ZoomedItem.Transform.localPosition;
        zoomedItemCachedRotation = ZoomedItem.Transform.localRotation;
        ZoomedItem.gameObject.SetLayerRecursively(overlayLayer);
        
        foreach (Item item in ZoomedItem.GetComponentsInChildren<Item>())
            item.DisablePhysics();

        ZoomedItem.Transform.parent = zoomedItemPivot;
        yield return Coroutines.MoveLocalRealtime(takeDuration, ZoomedItem.Transform, Vector3.zero, Quaternion.identity);
        TimeManager.Pause();
        zoomInItemRoutine = null;
    }

    IEnumerator ZoomOutItemRoutine()
    {
        Item zoomingOutItem = ZoomedItem;
        ZoomedItem = null;
        TimeManager.Unpause();
        
        zoomingOutItem.Transform.parent = zoomedItemCachedParent;
        
        yield return Coroutines.MoveLocalRealtime(takeDuration, zoomingOutItem.Transform, zoomedItemCachedPosition, zoomedItemCachedRotation);

        if (zoomedItemCachedParent == heldItemPivot)
        {
            HeldItem = zoomingOutItem as MovableItem;
        }
        else
        {
            //should not enable physics for held item
            foreach (Item item in zoomingOutItem.GetComponentsInChildren<Item>())
                item.EnablePhysics();
        }
        
        zoomedItemCachedParent = null;
        zoomedItemCachedPosition = Vector3.zero;
        zoomedItemCachedRotation = Quaternion.identity;
        
        zoomingOutItem.gameObject.SetLayerRecursively(defaultLayer);
        inputBuffer.SetPlayerActionMap();
        zoomOutItemRoutine = null;
    }

    void StopZoomInRoutine()
    {
        if(zoomInItemRoutine == null)
            return;
        
        StopCoroutine(zoomInItemRoutine);
        zoomInItemRoutine = null;
    }
    
    void StopTakeItemRoutine()
    {
        if(takeItemRoutine == null)
            return;
        
        StopCoroutine(takeItemRoutine);
        takeItemRoutine = null;
    }

    void PutIntoInventory()
    {
        if(HeldItem == null)
            return;
            
        inventory.AddItem(HeldItem);
        HeldItem.gameObject.SetActive(false);
        HeldItem = null;
    }

    void TakeFromInventory(int index)
    {
        if (inventory.GetItem(index) is not MovableItem item)
            return;

        inventory.RemoveItem(index);
        
        if(HeldItem != null)
            PutIntoInventory();

        HeldItem = item;
        HeldItem.gameObject.SetActive(true);
        HeldItem.Transform.localPosition = Vector3.zero;
        HeldItem.Transform.localRotation = Quaternion.identity;
        
        itemGhost.gameObject.SetLayerRecursively(IsZoomed ? overlayLayer : defaultLayer);
    }
}
