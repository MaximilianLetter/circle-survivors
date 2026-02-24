using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeFullscreenColor : MonoBehaviour
{
    private RawImage _fadeImage;
    [SerializeField] private Color _fadeColor = Color.black;

    private void Awake()
    {
        _fadeImage = GetComponent<RawImage>();
    }

    public IEnumerator FadeIn(float duration = 1f)
    {
        float elapsedTime = 0f;
        Color startColor = _fadeImage.color;
        Color endColor = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, 1f);

        while (elapsedTime < duration)
        {
            _fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _fadeImage.color = endColor;
    }

    public IEnumerator FadeOut(float duration = 1f)
    {
        float elapsedTime = 0f;
        Color startColor = _fadeImage.color;
        Color endColor = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, 0f);

        while (elapsedTime < duration)
        {
            _fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _fadeImage.color = endColor;
    }

    public void SetAlpha(float val)
    {
        Color col = _fadeImage.color;
        Color adjustedCol = new Color(col.r, col.g, col.b, val);

        _fadeImage.color = adjustedCol;
    }
}
