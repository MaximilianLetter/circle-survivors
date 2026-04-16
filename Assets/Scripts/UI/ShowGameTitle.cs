using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShowGameTitle : MonoBehaviour
{
    [SerializeField] private float _initialWait = 0.5f;
    [SerializeField] private float _fadeInTime = 2f;

    [SerializeField] private DecalProjector _decal;

    private Material _instanceMaterial;

    private IEnumerator Start()
    {
        _instanceMaterial = new Material(_decal.material);
        _decal.material = _instanceMaterial;

        _instanceMaterial.SetFloat("_Reveal", 0);

        yield return new WaitForSeconds(_initialWait);

        SoundManager.PlaySound(SoundManager.Instance.Library.WriteLong);

        float t = 0;

        while (t < _fadeInTime)
        {
            t += Time.deltaTime;

            float progress = t / _fadeInTime;

            _instanceMaterial.SetFloat("_Reveal", progress);

            yield return null;
        }
    }
}
