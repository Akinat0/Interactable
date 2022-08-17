
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class UIInteractableCanvas : MonoBehaviour
{
    [SerializeField] Interactor interactor;
    
    [Space]
    [SerializeField] RectTransform aimRoot;
    [SerializeField] Image aimImageFocused;
    [SerializeField] Image aimImageUnfocused;

    [Space]
    [SerializeField] GameObject itemDescriptionPanel;
    [SerializeField] TextMeshProUGUI itemDescriptionText;

    [Space] 
    [SerializeField] RectTransform actionsContainer;
    [SerializeField] UIInteractableAction actionPrefab;
    [SerializeField] UIIcons icons;
    
    
    readonly List<UIInteractableAction> ActiveActions = new ();
    ObjectPool<UIInteractableAction> actionsPool;

    void Start()
    {
        SetFocused(false);
        SetItemId(null);
        SetAim(true);
        
        actionsPool = new ObjectPool<UIInteractableAction>(CreateAction, GetAction, ReleaseAction, defaultCapacity:2);
    }

    void Update()
    {
        Item displayItem = GetDisplayItem();
        
        SetFocused(interactor.SelectedItem != null);
        SetPointerPosition(interactor.Pointer.Position);
        SetItemId(displayItem ? displayItem.Id : null);
        
        ClearActions();
        
        if(displayItem == null)
            return;
        
        foreach (ItemAction action in displayItem.GetActions())
        {
            string description = action.GetDescription(interactor);
            if(description != null)
                AddAction(action.Control, description);
        }
    }

    void OnDestroy()
    {
        actionsPool.Dispose();
    }

    public void SetAim(bool isShowAim)
    {
        aimRoot.gameObject.SetActive(isShowAim);
    }
    
    public void SetFocused(bool isFocused)
    {
        aimImageFocused.gameObject.SetActive(isFocused);
        aimImageUnfocused.gameObject.SetActive(!isFocused);
    }

    public void SetPointerPosition(Vector2 pointerPosition)
    {
        aimRoot.position = pointerPosition;
    }

    public void SetItemId([CanBeNull] string itemId)
    {
        if (itemId == null)
        {
            itemDescriptionPanel.SetActive(false);
            return;
        }
        
        itemDescriptionPanel.SetActive(true);
        itemDescriptionText.text = itemId;
    }

    public void ClearActions()
    {
        //put actions in reversed order to avoid text mesh recreations
        for (int i = ActiveActions.Count - 1; i >= 0; i--)
            actionsPool.Release(ActiveActions[i]);

        ActiveActions.Clear();
    }
    
    public void AddAction(Control control, string text)
    {
        UIInteractableAction action = actionsPool.Get();
        ActiveActions.Add(action);
        action.Icon = icons.Get(control);
        action.Text = text;
    }
    
    Item GetDisplayItem()
    {
        return interactor.ZoomedItem != null 
            ? interactor.ZoomedItem 
            : interactor.HeldItem != null 
                ? interactor.HeldItem 
                : interactor.SelectedItem != null 
                    ? interactor.SelectedItem 
                    : null;
    }
    
    UIInteractableAction CreateAction()
    {
        UIInteractableAction action = Instantiate(actionPrefab, actionsContainer);
        return action;
    }

    void GetAction(UIInteractableAction action)
    {
        action.gameObject.SetActive(true);
    }

    void ReleaseAction(UIInteractableAction action)
    {
        action.gameObject.SetActive(false);
    }
}
