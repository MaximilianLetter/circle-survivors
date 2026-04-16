using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FadeDecal : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 1f;

    private DecalProjector _decal;
    private float _targetAlpha;
    private float _passedTime;

    private void Awake()
    {
        _decal = GetComponent<DecalProjector>();
        _targetAlpha = _decal.fadeFactor;
    }

    private void OnEnable()
    {
        _decal.fadeFactor = 0f;
        _passedTime = 0f;
    }

    private void Update()
    {
        if (_decal.fadeFactor < _targetAlpha)
        {
            _passedTime += Time.deltaTime;

            float progress = _passedTime / _fadeDuration;

            _decal.fadeFactor = Mathf.Lerp(0, _targetAlpha, progress);
        }
    }
}
