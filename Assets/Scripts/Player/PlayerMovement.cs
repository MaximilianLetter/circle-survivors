using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private InputActionReference _moveInput;
    [SerializeField] private InputActionReference _turnInput;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 360f;    // Rotation per sec
    [SerializeField] private float _minTurnSpeed = 180f;

    private Vector3 _moveDirection;
    private Quaternion _turnDirection = Quaternion.identity;

    private void Update()
    {
        _moveDirection = _moveInput.action.ReadValue<Vector2>();

        // Get mouse pointing position
        // NOTE: only works for Keyboard + Mouse, not for Gamepad
        Ray ray = _cam.ScreenPointToRay(_turnInput.action.ReadValue<Vector2>());
        if (Physics.Raycast(ray, out RaycastHit hit, 100, _groundLayer))
        {
            Vector3 targetPos = hit.point;
            Vector3 dir = targetPos - transform.position;
            dir.y = 0f;


            if (dir != Vector3.zero)
            {
                _turnDirection = Quaternion.LookRotation(dir);
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
        Turn();
    }

    void Move()
    {
        Vector3 moveDirOnPlane = new Vector3(_moveDirection.x, 0, _moveDirection.y).ToIso();

        Vector3 targetPos = _rb.position + moveDirOnPlane * _moveSpeed * Time.fixedDeltaTime;

        //_rb.linearVelocity = moveDirOnPlane * _moveSpeed;
        _rb.MovePosition(targetPos);
    }

    void Turn()
    {
        Quaternion newRotation = Quaternion.RotateTowards(
            _rb.rotation,
            _turnDirection,
            _turnSpeed * Time.fixedDeltaTime
        );

        _rb.MoveRotation(newRotation);
    }

    public void AdjustTurnSpeed(int amountOfCharacters)
    {
        float newTurnSpeed = Mathf.Clamp(360f - amountOfCharacters * 30, _minTurnSpeed, 360f);
        _turnSpeed = newTurnSpeed;
    }
}
