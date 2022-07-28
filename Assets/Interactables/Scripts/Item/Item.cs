using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Item : MonoBehaviour
{
    [SerializeField] string itemId;
    [SerializeField] bool isZoomable = true;
    
    protected Transform cachedTransform;
    public Transform Transform => cachedTransform ? cachedTransform : cachedTransform = transform;

    Rigidbody cachedRigidbody;
    protected Rigidbody Rigidbody => cachedRigidbody ? cachedRigidbody : cachedRigidbody = GetComponent<Rigidbody>();

    bool cachedIsKinematic;

    public bool IsZoomable => isZoomable;

    public string Name => !string.IsNullOrEmpty(itemId) ? itemId : gameObject.name;

    void Start()
    {
        cachedIsKinematic = Rigidbody.isKinematic;
    }

    public void DisablePhysics()
    {
        Rigidbody.isKinematic = true;
    }

    public void EnablePhysics()
    {
        Rigidbody.isKinematic = cachedIsKinematic;
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
