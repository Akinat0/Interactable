using UnityEngine;


[RequireComponent(typeof(Item))]
public abstract class ItemAction : MonoBehaviour
{
    [SerializeField] bool isEnabled = true;
    
    public abstract Control Control { get; }

    public bool IsEnabled 
    { 
        get => isEnabled; 
        set => isEnabled = value; 
    }

    protected Item Item { get; private set; }

    void Awake()
    {
        Item = GetComponent<Item>();
    }
    
    public virtual bool CanProcess(Interactor interactor, Control control) => IsEnabled && Control == control;
    public abstract void Process(Interactor interactor);

    public abstract string GetDescription(Interactor interactor);

}
