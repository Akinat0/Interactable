
using UnityEngine;
using UnityEngine.Events;

public class InteractAction : ItemAction
{
    [SerializeField] UnityEvent onInteract;

    public override Control Control => Control.LeftMouseButton;

    public override void Process(Interactor interactor)
    {
        onInteract?.Invoke();
    }

    public override string GetDescription(Interactor interactor)
        => "Interact"; 
    
}
