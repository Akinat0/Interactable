
using UnityEngine;

[DisallowMultipleComponent]
public class MoveAction : ItemAction
{
    GameObject ghost;

    bool HeldItemAnimationInProgress;

    public override Control Control => Control.LeftMouseButton;

    public override bool CanProcess(Interactor interactor, Control control)
    {
        return base.CanProcess(interactor, control) && interactor.HeldItem == null;
    }

    public override void Process(Interactor interactor)
    {
        interactor.HeldItem = Item;
    }

    public override string GetDescription(Interactor interactor)
    {
        return interactor.HeldItem == Item ? "Place" : "Take";
    }

    public void StartMoving(Interactor interactor)
    {
        interactor.HeldItem.DisablePhysics();
        interactor.HeldItem.DisableCollisions();
        interactor.HeldItem.gameObject.SetLayerRecursively(Layers.CharacterLayer);
        
        interactor.GhostController.SetItem(Item);
        interactor.InteractorAnimator.PlayHeldItemAnimation(Item);
    }
    
    public void StopMoving(Interactor interactor)
    {
        interactor.GhostController.ReleaseItem();
        interactor.InteractorAnimator.StopAnimation();
    }
    
    public Transform GetGhost()
    {
        if (ghost == null)
        {
            ghost = Instantiate(gameObject, Vector3.zero, Quaternion.identity);

            Rigidbody ghostRigidbody = ghost.GetComponent<Rigidbody>();

            ghostRigidbody.isKinematic = true;
            ghostRigidbody.detectCollisions = false;

            ghost.SetLayerRecursively(LayerMask.NameToLayer("Ignore Raycast"));

            Material ghostMaterial = Resources.Load<Material>("Materials/GhostMaterial");

            foreach (MeshRenderer meshRenderer in ghost.GetComponentsInChildren<MeshRenderer>(true))
                meshRenderer.material = ghostMaterial;
        }
        
        ghost.SetActive(true);
        return ghost.transform;
    }

    public void ReleaseGhost()
    {
        if(ghost == null)
            return;
        
        ghost.SetActive(false);
    }
}
