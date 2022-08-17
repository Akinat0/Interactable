using UnityEngine;
using UnityEngine.InputSystem;

public class InputBuffer : MonoBehaviour
{
    PlayerInput playerInput;

    CursorLockMode cursorLockMode;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = cursorLockMode = CursorLockMode.Locked;
    }
    
    public bool IsZoomIn { get; set; }
    public bool IsZoomOut { get; set; }
    public bool IsInteract { get;  set; }
    public bool IsDeselectInput { get; set; }
    public bool IsTakeInput { get; set; }
    public bool IsInventoryInput { get; set; }
    public int InventoryInputValue { get; set; }
    
    public Vector2 PtrPosition { get; private set; }

    public void SetZoomedItemActionsMap()
    {
        Cursor.visible = false;
        Cursor.lockState = cursorLockMode = CursorLockMode.Confined;
        playerInput.SwitchCurrentActionMap("ZoomedItem");
    }

    public void SetPlayerActionMap()
    {
        Cursor.lockState = cursorLockMode = CursorLockMode.Locked;
        playerInput.SwitchCurrentActionMap("Player");
    }

    #region input system events
    
    void OnInteractInput()
    {
        IsInteract = true;
    }
    
    void OnZoomInput()
    {
        IsZoomIn = true;
    }

    void OnZoomOutInput()
    {
        IsZoomOut = true;
    }

    void OnDeselectInput()
    {
        IsDeselectInput = true;
    }

    void OnPtrPositionInput(InputValue input)
    {
        PtrPosition = input.Get<Vector2>();
    }

    void OnTakeInput()
    {
        IsTakeInput = true;
    }

    void OnInventoryInput(InputValue input)
    {
        IsInventoryInput = true;
        InventoryInputValue = (int) input.Get<float>();
        Debug.Log(InventoryInputValue);
    }
    
    #endregion
    
    void OnApplicationFocus(bool hasFocus)
    {
        if(hasFocus)
            Cursor.lockState = cursorLockMode;
    }
}




