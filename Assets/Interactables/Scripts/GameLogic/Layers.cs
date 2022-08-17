
using UnityEngine;

public static class Layers
{
    public static int OverlayLayer => LayerMask.NameToLayer("Overlay");
    public static int DefaultLayer => LayerMask.NameToLayer("Default");
    public static int CharacterLayer => LayerMask.NameToLayer("Character");
    
    public static LayerMask OverlayLayerMask => LayerMask.GetMask("Overlay");
    public static LayerMask DefaultRaycastLayersMask => Physics.DefaultRaycastLayers;
}
