using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldTextDecal
{
    private readonly DecalProjector _projector;
    private Material _mat;
    private RenderTexture _rt;
    private readonly MonoBehaviour _runner;
    private readonly float _maxOpacity;

    private float _currentFade = 0f;
    private Coroutine _activeCoroutine;

    public WorldTextDecal(
        DecalProjector projector,
        Material mat,
        RenderTexture rt,
        Vector2 worldSize,
        MonoBehaviour runner,
        float maxOpacity = 0.75f)
    {
        _projector = projector;
        _mat = mat;
        _rt = rt;
        _runner = runner;
        _maxOpacity = maxOpacity;
        _projector.material = mat;
        _projector.size = new Vector3(worldSize.x, worldSize.y, 0.5f);
        _projector.fadeFactor = 0f;
        _mat.SetFloat("_Reveal", 0f);
    }

    public void UpdateContent(Material newMat, RenderTexture newRt, Vector2 newWorldSize, Vector3? position = null)
    {
        // Clean up old resources
        if (_mat != null) Object.Destroy(_mat);
        if (_rt != null) { _rt.Release(); Object.Destroy(_rt); }

        _mat = newMat;
        _rt = newRt;

        _projector.material = newMat;
        _projector.size = new Vector3(newWorldSize.x, newWorldSize.y, _projector.size.z);
        _projector.fadeFactor = _currentFade * _maxOpacity;
        newMat.SetFloat("_Reveal", _currentFade);

        if (position.HasValue)
            _projector.transform.localPosition = position.Value;
    }

    public Coroutine FadeIn(float fadeTime)
    {
        StopActive();
        _activeCoroutine = _runner.StartCoroutine(FadeRoutine(_currentFade, 1f, fadeTime));
        return _activeCoroutine;
    }

    public Coroutine FadeOut(float fadeTime)
    {
        StopActive();
        _activeCoroutine = _runner.StartCoroutine(FadeRoutine(_currentFade, 0f, fadeTime));
        return _activeCoroutine;
    }

    public IEnumerator FadeInThenHoldThenOut(float fadeTime, float holdDuration)
    {
        yield return FadeIn(fadeTime);
        yield return new WaitForSeconds(holdDuration);
        yield return FadeOut(fadeTime);
        Destroy();
    }

    public IEnumerator FadeOutAndDestroy(float fadeTime)
    {
        yield return FadeOut(fadeTime);
        Destroy();
    }

    public void Destroy()
    {
        StopActive();
        if (_projector != null) Object.Destroy(_projector.gameObject);
        if (_mat != null) Object.Destroy(_mat);
        if (_rt != null) { _rt.Release(); Object.Destroy(_rt); }
    }

    private void StopActive()
    {
        if (_activeCoroutine != null)
        {
            _runner.StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }
    }

    private IEnumerator FadeRoutine(float from, float to, float fadeTime)
    {
        // Scale duration so a partial fade doesn't take the full fadeTime.
        // e.g. fading in from 0.8 only takes 20% of the time, not 100%.
        float duration = fadeTime * Mathf.Abs(to - from);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            _currentFade = Mathf.SmoothStep(from, to, t / duration);
            _projector.fadeFactor = _currentFade * _maxOpacity;
            _mat.SetFloat("_Reveal", _currentFade);
            yield return null;
        }

        _currentFade = to;
        _projector.fadeFactor = to * _maxOpacity;
        _mat.SetFloat("_Reveal", to);
        _activeCoroutine = null;
    }
}