using TMPro;
using UnityEngine;


public class SetMenuItemText : MonoBehaviour
{
    [SerializeField] private ToggleTextOnPlayerNearby _textInteraction;

    [TextArea(3, 5)]
    [SerializeField] private string _titleText;

    private void Start()
    {
        _textInteraction.SetTextContent(_titleText);
    }
}
