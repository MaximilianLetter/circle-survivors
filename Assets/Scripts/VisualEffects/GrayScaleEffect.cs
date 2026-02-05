using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

public class GrayScaleEffect : MonoBehaviour
{
    [SerializeField] private Volume _globalVolume;

    private ColorAdjustments _colorAdjust;
    private Coroutine _fadeRoutine;

    void Awake()
    {
        if (!_globalVolume.profile.TryGet(out _colorAdjust))
        {
            Debug.LogError("Color Adjustments not found on Volume!");
            enabled = false;
            return;
        }

        _colorAdjust.saturation.value = 0f;
    }

    public void SetToGray()
    {
        _colorAdjust.saturation.value = -100f;
    }

    public void FadeToGray(float duration = 1f) => StartFade(-100f, duration);
    public void FadeToColor(float duration = 1f) => StartFade(0f, duration);

    private void StartFade(float targetValue, float duration)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeVolume(targetValue, duration));
    }

    private IEnumerator FadeVolume(float target, float duration)
    {
        float start = _colorAdjust.saturation.value;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            _colorAdjust.saturation.value = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        _colorAdjust.saturation.value = target;
        _fadeRoutine = null;
    }
}
