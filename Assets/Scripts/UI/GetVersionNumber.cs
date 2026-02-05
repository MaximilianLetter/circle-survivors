using TMPro;
using UnityEngine;

public class GetVersionNumber : MonoBehaviour
{
    private TextMeshProUGUI _textField;

    private void Start()
    {
        _textField = GetComponent<TextMeshProUGUI>();

        _textField.text = "v" + Application.version;
    }
}
