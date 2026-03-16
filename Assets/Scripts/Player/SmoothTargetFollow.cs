using System.Collections;
using UnityEngine;

public class SmoothTargetFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.2f;

    [Header("Optional Rotation Properties")]
    [SerializeField] private bool _doRotate = false;
    [SerializeField] private float _rotationStrength = 4f;
    //[SerializeField] private float _rotationAmount = 10f;
    //[SerializeField] private float _rotationSmoothTime = 0.2f;

    private Vector3 _offset;

    // Movement
    private Vector3 _currentVelocity = Vector3.zero;

    // Rotation
    private Quaternion _baseRotation;
    private Quaternion _rotationOffset;

    // Offsets
    private Vector3 _effectOffset = Vector3.zero;

    private void Awake()
    {
        _offset = transform.position - _target.position;
        _baseRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = _target.position + _offset;

        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _currentVelocity,
            _smoothTime
        );

        transform.position = smoothPosition + _effectOffset;


        // NOTE: only used in menu currently
        if (!_doRotate) return;

        // NOTE: hacky height increase because of otherwise focus on ground always
        Vector3 worldDir = (_target.position + new Vector3(0, 2, 0)) - transform.position;
        Vector3 localDir = Quaternion.Inverse(_baseRotation) * worldDir;

        float yaw = localDir.x * _rotationStrength;
        float pitch = -localDir.y * _rotationStrength;

        Quaternion targetOffset = Quaternion.Euler(pitch, yaw, 0f);

        _rotationOffset = Quaternion.Slerp(
            _rotationOffset,
            targetOffset,
            Time.deltaTime * _smoothTime
        );

        transform.rotation = _baseRotation * _rotationOffset;
    }

    /// <summary>
    /// Can be set from outside to offset the following object.
    /// </summary>
    /// <param name="offset"></param>
    public void SetEffectOffset(Vector3 offset)
    {
        _effectOffset = offset;
    }

    public void JumpToTarget()
    {
        transform.position = _target.position + _offset;
    }
}
