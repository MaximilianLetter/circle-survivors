using UnityEngine;

public class SmoothCameraSizeAdjustment : MonoBehaviour
{
    [SerializeField] private float _cameraSize = 5f;
    [SerializeField] private float _cameraSizeStep = 0.5f;
    [SerializeField] private float _transitionTime = 2f;

    private bool _transition = false;
    private float _startSize;
    private float _targetSize;
    private float _elapsedTime = 0f;

    private Camera _cam;

    private void Start()
    {
        _cam = GetComponent<Camera>();
    }

    // NOTE: could be moved into a Coroutine to have less update calls
    private void Update()
    {
        if (!_transition) return;

        _elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsedTime / _transitionTime);

        _cam.orthographicSize = Mathf.Lerp(_startSize, _targetSize, Mathf.SmoothStep(0f, 1f, t));

        if (t >= 1f)
        {
            _transition = false;
            _elapsedTime = 0f;
        }
    }

    public void AdjustCameraSize(bool increase)
    {
        _cameraSize += _cameraSizeStep * (increase ? 1 : -1);

        TransitionSizeTo(_cameraSize);
    }

    public void TransitionSizeTo(float size)
    {
        _startSize = _cam.orthographicSize;
        _targetSize = size;
        _elapsedTime = 0f;
        _transition = true;
    }
}
