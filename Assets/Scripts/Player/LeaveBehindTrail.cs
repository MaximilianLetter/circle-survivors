using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LeaveBehindTrail : MonoBehaviour
{
    [SerializeField] private float _interval;

    [SerializeField] private float _fadeInDuration;
    [SerializeField] private float _fadeOutDuration;
    [SerializeField] private float _maxAlpha;

    [SerializeField] private DecalProjector _trailElement;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        InvokeRepeating(nameof(SpawnTrailObject), _interval, _interval);
    }

    private void SpawnTrailObject()
    {
        if (_rb.linearVelocity.magnitude < 1) return;

        Vector3 moveDir = _rb.linearVelocity.normalized;
        var decal = Instantiate(_trailElement, transform.position, Quaternion.LookRotation(moveDir) * Quaternion.Euler(90, 0, 0));
        StartCoroutine(AnimateDecal(decal));
    }

    private IEnumerator AnimateDecal(DecalProjector decal)
    {
        decal.fadeFactor = 0f;

        float t = 0f;
        while (t < _fadeInDuration)
        {
            float percent = t / _fadeInDuration;
            decal.fadeFactor = percent * _maxAlpha;

            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        t = 0f;
        while (t < _fadeOutDuration)
        {
            float percent = 1 - (t / _fadeOutDuration);
            decal.fadeFactor = percent * _maxAlpha;

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(decal.gameObject);
    }
}
