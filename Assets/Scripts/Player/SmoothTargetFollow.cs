using System.Collections;
using UnityEngine;

public class SmoothTargetFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime;

    private Vector3 _offset;
    private Vector3 _currentVelocity = Vector3.zero;

    private Vector3 _effectOffset = Vector3.zero;

    private void Awake()
    {
        _offset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = _target.position + _offset;

        Vector3 smoothPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _smoothTime);

        transform.position = smoothPosition + _effectOffset;
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
