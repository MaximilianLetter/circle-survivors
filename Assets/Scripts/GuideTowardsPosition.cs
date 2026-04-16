using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GuideTowardsTarget : MonoBehaviour
{
    private Transform _target;
    private Transform _objToGuide;
    [SerializeField] private float _indicatorDistance;           // NOTE: Could also use something like screen border 
    [SerializeField] private float _distancePuffer = 2f;
    [SerializeField] private DecalProjector _decal;
    [SerializeField] private float _decalAlpha = 0.25f;

    private bool _isGuiding;

    private void Start()
    {
        _objToGuide = GameObject.FindWithTag("Player").transform;
    }

    private Coroutine _fadeRoutine;

    public void StartGuidance()
    {
        _isGuiding = true;

        FadeTo(_decalAlpha, 2f);
    }

    public void StopGuidance()
    {
        _isGuiding = false;

        FadeTo(0, 2f, true);
    }

    private void FadeTo(float target, float duration, bool destroyAfterwards = false)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeRoutine(target, duration, destroyAfterwards));
    }

    private IEnumerator FadeRoutine(float target, float duration, bool destroyAfterwards)
    {
        float start = _decal.fadeFactor;
        float time = 0f;

        target = Mathf.Clamp01(target);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            _decal.fadeFactor = Mathf.Lerp(start, target, t);
            yield return null;
        }

        _decal.fadeFactor = target;
        _fadeRoutine = null;

        if (destroyAfterwards) Destroy(gameObject);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void Update()
    {
        if (!_isGuiding) return;

        if (_target == null) return;

        Vector3 direction = _target.position - _objToGuide.position;
        direction.Normalize();

        float distanceToTarget = Vector3.Distance(_target.position, _objToGuide.position);
        float distanceToShow = _indicatorDistance;

        if ((distanceToTarget - _distancePuffer) < distanceToShow)
        {
            distanceToShow = distanceToTarget - _distancePuffer;
        }

        transform.position = _objToGuide.position + direction * distanceToShow;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
