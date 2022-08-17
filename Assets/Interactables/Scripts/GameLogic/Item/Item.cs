using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    [SerializeField] string itemId;

    public Transform Transform { get; private set; }
    
    public Rigidbody Rigidbody { get; private set; }
    public bool CachedIsKinematic { get; set; }

    public string Id => !string.IsNullOrEmpty(itemId) ? itemId : gameObject.name;

    ItemAction[] itemActions;

    void Start()
    {
        Transform = GetComponent<Transform>();
        Rigidbody = GetComponent<Rigidbody>();
        CachedIsKinematic = Rigidbody.isKinematic;
        itemActions = GetComponents<ItemAction>();
    }

    public bool TryGetAction<TAction>(out TAction action) where TAction : ItemAction
    {
        action = GetAction<TAction>();
        return action != null;
    }
    
    public TAction GetAction<TAction>(bool includeDisabled = false) where TAction : ItemAction
    {
        return includeDisabled 
            ? itemActions.OfType<TAction>().FirstOrDefault()
            : itemActions.OfType<TAction>().FirstOrDefault(action => action.IsEnabled);
    }

    public ItemAction GetSuitableAction(Interactor interactor, Control control)
    {
        return itemActions.FirstOrDefault(action => action.CanProcess(interactor, control));
    }

    public IReadOnlyCollection<ItemAction> GetActions() => itemActions;
    public IEnumerable<TAction> GetActions<TAction>() where TAction : ItemAction 
        => GetActions().OfType<TAction>();

    public bool TryGetSuitableAction(Interactor interactor, Control control, out ItemAction action)
    {
        action = GetSuitableAction(interactor, control);
        return action != null;
    }

    public void DisablePhysics()
    {
        Rigidbody.isKinematic = true;
    }

    public void EnablePhysics()
    {
        Rigidbody.isKinematic = CachedIsKinematic;
    }

    public void DisableCollisions()
    {
        Rigidbody.detectCollisions = false;
    }

    public void EnableCollisions()
    {
        Rigidbody.detectCollisions = true;
    }
}
