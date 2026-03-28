using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WorldTextFactory : MonoBehaviour
{
    [SerializeField] private Material _decalBaseMaterial;
    [SerializeField] private DecalProjector _decalPrefab;
    [SerializeField] private TextToRenderTexture _textRendererPrefab;
    [SerializeField] private float _baseWidth = 8f;
    [SerializeField] private float _padding = 1.15f;
    [SerializeField] private float _maxOpacity = 0.75f;

    public WorldTextDecal Create(
        string text,
        Vector3 position,
        MonoBehaviour runner,
        Transform parent = null,
        float yRot = 45f,
        float fontSize = 6f,
        float? width = null)
    {
        var textRenderer = Instantiate(_textRendererPrefab);
        Bounds bounds = textRenderer.SetTextProps(text, fontSize, width.HasValue ? width.Value : _baseWidth);
        RenderTexture rt = textRenderer.RenderToRT(bounds, _padding, out Vector2 worldSize);
        Destroy(textRenderer.gameObject);

        Material mat = new Material(_decalBaseMaterial);
        mat.SetTexture("_Base_Map", rt);

        DecalProjector projector = Instantiate(
            _decalPrefab,
            position,
            Quaternion.Euler(90f, yRot, 0f),
            parent
        );

        if (parent)
            projector.transform.localPosition = position;

        return new WorldTextDecal(projector, mat, rt, worldSize, runner, _maxOpacity);
    }

    public void UpdateDecal(WorldTextDecal decal, string newText, Vector3? position = null, float fontSize = 6f)
    {
        var textRenderer = Instantiate(_textRendererPrefab);
        Bounds bounds = textRenderer.SetTextProps(newText, fontSize, _baseWidth);
        RenderTexture rt = textRenderer.RenderToRT(bounds, _padding, out Vector2 worldSize);
        Destroy(textRenderer.gameObject);

        Material mat = new Material(_decalBaseMaterial);
        mat.SetTexture("_Base_Map", rt);

        decal.UpdateContent(mat, rt, worldSize, position);
    }
}