using UnityEngine;

public class FadeOutFx : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.15f;
    private float _timer;
    private Material _mat;

    // NOTE:
    // This is all placeholder for using animator instead

    private void Awake()
    {
        _mat = GetComponentInChildren<MeshRenderer>().material;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _timer = 0f;
        SetAlpha(1f);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float t = _timer / _lifetime;

        SetAlpha(1f - t);

        if (t >= 1f) gameObject.SetActive(false);
    }

    private void SetAlpha(float a)
    {
        Color c = _mat.color;
        c.a = a;
        _mat.color = c;
    }
}
