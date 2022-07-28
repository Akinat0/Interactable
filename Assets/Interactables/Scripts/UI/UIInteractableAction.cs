using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractableAction : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI text;

    public Sprite Icon
    {
        get => icon.sprite;
        set => icon.sprite = value;
    }

    public string Text
    {
        get => text.text;
        set => text.text = value;
    }
}
