using TMPro;
using UnityEngine;


public class SetMenuItemText : MonoBehaviour
{
    [SerializeField] private Transform _worldCanvas;
    [SerializeField] private TextMeshProUGUI _titleObj;
    [SerializeField] private TextMeshProUGUI _descriptionObj;

    [TextArea(3, 5)]
    [SerializeField] private string _titleText;
    [TextArea(3, 5)]
    [SerializeField] private string _descriptionText;

    [Header("Layout")]
    [SerializeField] private TextAlignmentOptions _align;
    [SerializeField] private float _xOffset = 3.5f;

    private void Start()
    {
        _titleObj.text = _titleText;
        _descriptionObj.text = _descriptionText;

        // NOTE: Alignment is not just Left & Right, its TopLeft, BottomLeft, ...
        // -> Assume everything is Top-Aligned for the moment
        if (_align == TextAlignmentOptions.TopRight)
        {
            _titleObj.alignment = TextAlignmentOptions.BottomLeft;
            _descriptionObj.alignment = TextAlignmentOptions.TopLeft;

            var currPos = _worldCanvas.localPosition;
            currPos.x = _xOffset;
            _worldCanvas.localPosition = currPos;
        }

        if (_align == TextAlignmentOptions.TopLeft)
        {
            _titleObj.alignment = TextAlignmentOptions.BottomRight;
            _descriptionObj.alignment = TextAlignmentOptions.TopRight;

            var currPos = _worldCanvas.localPosition;
            currPos.x = -_xOffset;
            _worldCanvas.localPosition = currPos;
        }
    }
}
