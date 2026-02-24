using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private FadeCanvasGroup _fade;
    [SerializeField] private TextMeshProUGUI _textObj;

    public void SetCharacterAmountText(int current, int limit)
    {
        _textObj.text = current.ToString() + "/" + limit.ToString();
    }

    public FadeCanvasGroup GetFadeGroup()
    {
        return _fade;
    }
}
