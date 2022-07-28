using UnityEngine;
using UnityEngine.InputSystem;

public class InputBuffer : MonoBehaviour
{
    public static InputBuffer Instance { get; set; }

    PlayerInput playerInput;

    CursorLockMode cursorLockMode;

    void Awake()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = Instance.cursorLockMode = CursorLockMode.Locked;
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
        Cursor.lockState = Instance.cursorLockMode = CursorLockMode.Confined;
        Instance.playerInput.SwitchCurrentActionMap("ZoomedItem");
    }

    public void SetPlayerActionMap()
    {
        Cursor.lockState = Instance.cursorLockMode = CursorLockMode.Locked;
        Instance.playerInput.SwitchCurrentActionMap("Player");
    }
    
    //input system event
    void OnInteractInput()
    {
        IsInteract = true;
    }
    
    //input system event
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
    
    void OnApplicationFocus(bool hasFocus)
    {
        if(hasFocus)
            Cursor.lockState = Instance.cursorLockMode;
    }
}




