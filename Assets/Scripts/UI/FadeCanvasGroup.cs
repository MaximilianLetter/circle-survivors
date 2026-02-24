using UnityEngine;

public class FadeCanvasGroup : MonoBehaviour
{
    [SerializeField] private float _fadeSpeed = 2f;

    private CanvasGroup _canvasGroup;
    private float _targetAlpha;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, _fadeSpeed * Time.deltaTime);

        if (_canvasGroup.alpha < 0.002f && _targetAlpha == 0f)
        {
            gameObject.SetActive(false);
        }
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);

        _targetAlpha = 1f;
    }

    public void FadeOut()
    {
        _targetAlpha = 0f;
    }
}
