using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FillBoundsSetter : MonoBehaviour
{
    static readonly int FillMinYID = Shader.PropertyToID("_FillMinY");
    static readonly int FillMaxYID = Shader.PropertyToID("_FillMaxY");

    Renderer _renderer;
    MaterialPropertyBlock _mpb;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        UpdateBounds();
    }

    void UpdateBounds()
    {
        var bounds = _renderer.localBounds;

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FillMinYID, bounds.min.y);
        _mpb.SetFloat(FillMaxYID, bounds.max.y);
        _renderer.SetPropertyBlock(_mpb);
    }

    void OnValidate()
    {
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        UpdateBounds();
    }
}
