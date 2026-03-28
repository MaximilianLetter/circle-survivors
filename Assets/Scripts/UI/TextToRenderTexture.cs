using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TextToRenderTexture : MonoBehaviour
{
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private float _pixelsPerUnit = 100f; // This is resolution -> higher == sharper

    public float PixelsPerUnit => _pixelsPerUnit;

    public Bounds SetTextProps(string text, float fontSize, float width)
    {
        _text.text = text;
        _text.fontSize = fontSize;
        _text.rectTransform.sizeDelta = new Vector2(width, 1000f);
        _text.ForceMeshUpdate();

        return _text.mesh.bounds;
    }

    public RenderTexture RenderToRT(Bounds textBounds, float paddingFactor, out Vector2 worldSize)
    {
        float worldW = textBounds.size.x * paddingFactor;
        float worldH = textBounds.size.y * paddingFactor;

        int pixelW = Mathf.Clamp(Mathf.CeilToInt(worldW * _pixelsPerUnit), 64, 4096);
        int pixelH = Mathf.Clamp(Mathf.CeilToInt(worldH * _pixelsPerUnit), 64, 4096);

        RenderTexture rt = new RenderTexture(pixelW, pixelH, 0, RenderTextureFormat.ARGB32);
        rt.Create();

        Matrix4x4 centerOffset = Matrix4x4.Translate(-textBounds.center);

        float halfW = worldW * 0.5f;
        float halfH = worldH * 0.5f;
        Matrix4x4 ortho = Matrix4x4.Ortho(-halfW, halfW, -halfH, halfH, -1f, 1f);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        CommandBuffer cmd = new CommandBuffer { name = "TextToRT" };
        cmd.SetRenderTarget(rt);
        cmd.ClearRenderTarget(true, true, Color.clear);

        // Matrix magic by Claude
        cmd.SetProjectionMatrix(ortho);
        cmd.SetViewMatrix(Matrix4x4.Translate(-textBounds.center));
        cmd.DrawMesh(_text.mesh, Matrix4x4.identity, _text.fontMaterial);

        Graphics.ExecuteCommandBuffer(cmd);

        RenderTexture.active = prev;
        cmd.Dispose();

        _text.renderer.enabled = false;

        worldSize = new Vector2(worldW, worldH);
        return rt;
    }
}