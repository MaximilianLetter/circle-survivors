using UnityEngine;

public class FadeInAndOut : MonoBehaviour
{
    [SerializeField] private float _fadeDuration;
    [SerializeField] private float _holdDuration;

    private CanvasGroup _canvasGroup;
    private float _lifetime;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        _lifetime = 0f;
    }

    private void Update()
    {
        _lifetime += Time.deltaTime;

        if (_lifetime < _fadeDuration)
        {
            _canvasGroup.alpha = _lifetime / _fadeDuration;
        }

        if (_lifetime > _holdDuration)
        {
            float alpha = 1 - ((_lifetime - _holdDuration) / _fadeDuration);
            _canvasGroup.alpha = alpha;

            if (alpha < 0f)
            {
                _canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        }
    }
}
