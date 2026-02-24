using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class SpecialResourceIndicator : MonoBehaviour
{
    [SerializeField] private Image _fillerImage;
    [SerializeField] private SpecialResource _specialResource;
    [SerializeField] private float _animationDuration = 0.25f;

    [SerializeField] private Color _baseColor = Color.blue;
    [SerializeField] private Color _chargedColor = Color.blueViolet;
    [SerializeField] private Color _lockeddColor = Color.gray;

    private Coroutine _animationCoroutine;

    private void Start()
    {
        _fillerImage.color = _baseColor;
    }

    public void SetFillPercentage(float value, bool highlightOnFull = false, float givenDuration = 0)
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        float duration = givenDuration == 0 ? _animationDuration : givenDuration;

        _animationCoroutine = StartCoroutine(AnimateFill(value, highlightOnFull, duration));
    }

    private IEnumerator AnimateFill(float targetValue, bool highlightOnFull, float duration)
    {

        float startValue = _fillerImage.fillAmount;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            _fillerImage.fillAmount = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        _fillerImage.fillAmount = targetValue;

        if (targetValue == 1f)
        {
            if (highlightOnFull) StartCoroutine(LerpToColor(_chargedColor));
            else StartCoroutine(LerpToColor(_baseColor));
        }
        else if (targetValue == 0f)
        {
            if (highlightOnFull) StartCoroutine(LerpToColor(_baseColor));
            else StartCoroutine(LerpToColor(_lockeddColor));
        }

        _animationCoroutine = null;
    }

    private IEnumerator LerpToColor(Color targetCol)
    {
        Color startCol = _fillerImage.color;

        float elapsedTime = 0;
        while (elapsedTime < _animationDuration)
        {
            _fillerImage.color = Color.Lerp(startCol, targetCol, elapsedTime / _animationDuration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        _fillerImage.color = targetCol;
    }
}
