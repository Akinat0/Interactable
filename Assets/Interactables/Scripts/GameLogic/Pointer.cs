using System;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] InputBuffer inputBuffer;
    
    public bool IsLocked { get; set; }
    public Vector2 Position { get; private set; }

    void Awake()
    {
        IsLocked = true;
    }

    void Update()
    {
        Position = IsLocked
            ? new Vector2(Screen.width, Screen.height) / 2
            : inputBuffer.PtrPosition;
    }
}
