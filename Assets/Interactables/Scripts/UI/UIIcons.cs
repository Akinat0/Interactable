using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName="Icons", menuName = "Interactables/UIIcons")]
public class UIIcons : ScriptableObject
{
    [Serializable]
    struct Entry
    {
        [SerializeField] public Control Control;
        [SerializeField] public Sprite Icon;
    }

    [SerializeField] Entry[] entries;

    Dictionary<Control, Sprite> lut;

    public Sprite Get(Control control)
    {
        lut ??= entries.ToDictionary(entry => entry.Control, entry => entry.Icon);
        lut.TryGetValue(control, out Sprite icon);
        return icon;
    }

}

[Serializable]
public enum Control
{
    LeftMouseButton,
    RightMouseButton,
    Space,
}

