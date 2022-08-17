
using System.Linq;
using UnityEngine;

public class ItemGhostController : MonoBehaviour
{
    [SerializeField] PointerRaycaster raycaster;
    [SerializeField] Interactor interactor;

    Item currentItem;
    MoveAction currentItemAction;
    
    public void SetItem(Item item)
    {
        if(currentItem != null)
            ReleaseItem();
        
        currentItem = item;
        currentItemAction = currentItem.GetAction<MoveAction>();

        UpdateGhostPosition();
    }

    public void ReleaseItem()
    {
        currentItemAction.ReleaseGhost();
        currentItem = null;
    }

    void Update()
    {
        if(currentItem == null)
            return;
        
        UpdateGhostPosition();
    }

    void UpdateGhostPosition()
    {
        Transform ghost = currentItemAction.GetGhost();
        
        if (raycaster.IsEmpty)
        {
            ghost.parent = null;
            ghost.gameObject.SetActive(false);
            return;
        }

        if (raycaster.Buffer[0].collider.TryGetComponent(out Item item))
        {
            ReceiveAction receiver = 
                item.GetActions<ReceiveAction>().FirstOrDefault(action => action.IsEnabled && action.CanProcessItem(currentItem));

            if (receiver != null)
            {
                ghost.parent = receiver.ItemPivot;
                ghost.localPosition = Vector3.zero;
                ghost.localRotation = Quaternion.identity;
                ghost.gameObject.SetLayerRecursively(ghost.parent.gameObject.layer);
                ghost.gameObject.SetActive(true);
                return;
            }
        }
        
        //do not show ghost in zoom mode
        if (interactor.IsZoomed)
        {
            ghost.parent = null;
            ghost.gameObject.SetActive(false);
            return;
        }
        
        ghost.parent = null;
        ghost.position = raycaster.Buffer[0].point;
        ghost.up = raycaster.Buffer[0].normal;
        ghost.gameObject.SetLayerRecursively(Layers.DefaultLayer);
        ghost.gameObject.SetActive(true);
    }
}
