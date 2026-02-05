using UnityEngine;

[RequireComponent (typeof(BaseCharacter))]
public class HealthPointsIndicator : MonoBehaviour
{
    [Range(0f, 1f)]
    private float _fillPercent;

    static readonly int FillPercentID = Shader.PropertyToID("_FillPercent");

    Renderer[] _renderers;
    MaterialPropertyBlock _mpb;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();
    }

    public void SetHealthVisuals(float normalizedHealth)
    {
        _fillPercent = Mathf.Clamp01(normalizedHealth);
        ApplyFill();
    }

    private void ApplyFill()
    {
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetFloat(FillPercentID, _fillPercent);
            r.SetPropertyBlock(_mpb);
        }
    }

    void OnEnable()
    {
        ApplyFill();
    }
}
