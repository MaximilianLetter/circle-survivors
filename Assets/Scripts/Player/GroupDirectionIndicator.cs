using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

public class GroupDirectionIndicator : MonoBehaviour
{
    [SerializeField] private DecalProjector _decal;
    [SerializeField] private float _fadeDuration = 1f;
    [SerializeField] private float _targetOpacity = 0.6f;

    private Coroutine _fadeRoutine;
    private bool _isVisible;

    private void Start()
    {
        _decal.fadeFactor = _isVisible ? _targetOpacity : 0f;
    }

    public void ToggleVisibility(bool state)
    {
        if (_isVisible == state)
            return;

        _isVisible = state;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeRoutine(state ? _targetOpacity : 0f));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = _decal.fadeFactor;
        float time = 0f;

        while (time < _fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / _fadeDuration);

            _decal.fadeFactor = Mathf.Lerp(start, target, t);
            yield return null;
        }

        _decal.fadeFactor = target;
        _fadeRoutine = null;
    }
}
