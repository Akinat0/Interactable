
using TMPro;
using UnityEngine;

public class UIInventoryItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI indexText;
    [SerializeField] TextMeshProUGUI itemIdText;

    public string Name
    {
        get => itemIdText.text;
        set => itemIdText.text = value;
    }
    
    public void SetIndex(int index)
        => indexText.text = index.ToString();
}
