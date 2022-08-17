using System;
using System.Collections.Generic;
using UnityEngine;

public class PointerRaycaster : MonoBehaviour
{
    readonly IComparer<RaycastHit> HitComparer = 
        Comparer<RaycastHit>.Create((lhs, rhs) => lhs.distance.CompareTo(rhs.distance));
 
    public static PointerRaycaster Instance { get; private set; }
    
    [SerializeField] Pointer pointer;
    [SerializeField] Camera raycastCamera;
    [SerializeField] float raycastDistance;
    
    public int CollisionsCount { get; private set; }
    public bool IsEmpty { get; private set; }
    public LayerMask RaycastLayerMask { get; set; }
    
    RaycastHit[] hitBuffer = new RaycastHit[5];

    public IReadOnlyList<RaycastHit> Buffer => hitBuffer;

    void Awake()
    {
        Instance = this;
        RaycastLayerMask = Layers.DefaultRaycastLayersMask;
    }

    void Update()
    {
        for (int i = 0; i < hitBuffer.Length; i++)
            hitBuffer[i] = new RaycastHit();

        Ray ray = raycastCamera.ScreenPointToRay(pointer.Position);
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);
        CollisionsCount = Physics.RaycastNonAlloc(ray, hitBuffer, raycastDistance, RaycastLayerMask.value);
        IsEmpty = CollisionsCount == 0; 
        
        if(!IsEmpty)
            Array.Sort(hitBuffer, 0, Mathf.Min(hitBuffer.Length, CollisionsCount), HitComparer);
    }
    
}
