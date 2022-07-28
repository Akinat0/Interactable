using UnityEngine;
using UnityEngine.Events;

public class InteractableItem : Item
{
    [SerializeField] UnityEvent OnInteract;

    public void Interact()
    {
        OnInteract.Invoke();
    }
}
