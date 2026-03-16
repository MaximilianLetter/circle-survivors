using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private AnimationCurve _immediateCurve;
    [SerializeField] private AnimationCurve _smoothCurve;

    public static CameraShake Instance { get; private set; }

    private SmoothTargetFollow _follow;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _follow = GetComponent<SmoothTargetFollow>();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void TriggerShake(float duration = 3f, float magnitude = 0.025f, bool fadeIn = false)
    {
        StartCoroutine(Shake(duration, magnitude, fadeIn));
    }

    private IEnumerator Shake(float duration, float magnitude, bool fadeIn)
    {

        AnimationCurve curve = fadeIn ? _smoothCurve : _immediateCurve;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float percent = elapsed / duration;
            float damper = curve.Evaluate(percent);

            float appliedMagnitude = magnitude * damper;

            float x = Random.Range(-1f, 1f) * appliedMagnitude;
            float z = Random.Range(-1f, 1f) * appliedMagnitude;

            _follow.SetEffectOffset(new Vector3(x, 0f, z));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore the original offset
        _follow.SetEffectOffset(Vector3.zero);
    }
}
