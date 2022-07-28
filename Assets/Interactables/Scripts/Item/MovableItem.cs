using UnityEngine;

public class MovableItem : Item
{
    GameObject ghost;
    
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
