
using UnityEngine;

public class ZoomedItemController : MonoBehaviour
{
    [SerializeField] Pointer pointer;

    Vector2? previousDragPosition;
    Item currentItem;

    void Update()
    {
        if (currentItem == null)
            return;

        //Time scale equals to 0 while item zoomed, so we should sync transforms manually
        Physics.SyncTransforms();

        if(!previousDragPosition.HasValue)
            return;
        
        float diff = pointer.Position.x - previousDragPosition.Value.x;
        previousDragPosition = pointer.Position;
        currentItem.Transform.Rotate(0, -diff, 0);
        Physics.SyncTransforms();
    }

    public void SetZoomedItem(Item item)
    {
        currentItem = item;
    }

    public void BeginDrag()
    {
        previousDragPosition = pointer.Position;
    }

    public void EndDrag()
    {
        previousDragPosition = null;
    }
}
