using UnityEngine;

public class RandomScaleVariation : MonoBehaviour
{
    [SerializeField] private bool _useRandomScaling;
    [SerializeField] private Vector2 _scaleRange = new Vector2(0.75f, 1.25f);

    private void Start()
    {
        if (!_useRandomScaling) return;

        float scale = Random.Range(_scaleRange.x, _scaleRange.y);
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
