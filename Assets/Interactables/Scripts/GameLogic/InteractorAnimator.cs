using System;
using System.Collections;
using UnityEngine;

public class InteractorAnimator : MonoBehaviour
{
    [SerializeField] float takeAnimationDuration;
    [SerializeField] Transform heldItemPivot;
    
    [Space]
    [SerializeField] float zoomAnimationDuration;
    [SerializeField] Transform zoomedItemPivot;
    
    [Space]
    [SerializeField] PostProcessing postProcessing;

    IEnumerator currentAnimation;
    Action animationCallback;
    
    Item zoomedItem;
    Transform zoomedItemCachedParent;
    Vector3 zoomedItemCachedPosition;
    Quaternion zoomedItemCachedRotation;

    public void PlayHeldItemAnimation(Item item, Action finished = null)
    {
        StopAnimation();

        void Finished()
        {
            item.Transform.localPosition = Vector3.zero;
            item.Transform.localRotation = Quaternion.identity;
            finished?.Invoke();
        }
        
        animationCallback = Finished;
        
        item.Transform.parent = heldItemPivot;
        StartCoroutine(currentAnimation = Coroutines.MoveLocalRealtime(takeAnimationDuration, item.Transform, Vector3.zero,
            Quaternion.identity, InvokeAnimationCallback));
    }

    public void PlayZoomInItemAnimation(Item item, Action finished = null)
    {
        StopAnimation();
        
        void Finished()
        {
            item.Transform.localPosition = Vector3.zero;
            item.Transform.localRotation = Quaternion.identity;
            finished?.Invoke();
        }
        
        animationCallback = Finished;

        zoomedItem = item;
        zoomedItemCachedParent = zoomedItem.Transform.parent;
        zoomedItemCachedPosition = zoomedItem.Transform.localPosition;
        zoomedItemCachedRotation = zoomedItem.Transform.localRotation;

        zoomedItem.Transform.parent = zoomedItemPivot;
        postProcessing.SetBlurBackground(zoomAnimationDuration);

        StartCoroutine(currentAnimation = Coroutines.MoveLocalRealtime(zoomAnimationDuration, zoomedItem.Transform, Vector3.zero, Quaternion.identity, InvokeAnimationCallback));
    }

    public void PlayZoomOutItemAnimation(Action finished = null)
    {
        if(zoomedItem == null)
            return;
        
        StopAnimation();

        Item itemCaptured = zoomedItem;
        Vector3 positionCaptured = zoomedItemCachedPosition;
        Quaternion rotationCaptured = zoomedItemCachedRotation;
        
        void Finished()
        {
            itemCaptured.Transform.localPosition = positionCaptured;
            itemCaptured.Transform.localRotation = rotationCaptured;
            finished?.Invoke();
        }
        
        animationCallback = Finished;

        zoomedItem.Transform.parent = zoomedItemCachedParent;
        postProcessing.SetCommonBackground(zoomAnimationDuration);
        
        StartCoroutine(currentAnimation = Coroutines.MoveLocalRealtime(zoomAnimationDuration, zoomedItem.Transform,
            zoomedItemCachedPosition, zoomedItemCachedRotation, InvokeAnimationCallback));
        
        zoomedItem = null;
        zoomedItemCachedParent = null;
        zoomedItemCachedPosition = Vector3.zero;
        zoomedItemCachedRotation = Quaternion.identity;
    }

    public void StopAnimation()
    {
        if (currentAnimation == null) 
            return;
        
        StopCoroutine(currentAnimation);
        currentAnimation = null;
        InvokeAnimationCallback();
    }

    void InvokeAnimationCallback()
    {
        Action action = animationCallback;
        animationCallback = null;
        action?.Invoke();
    }
}
