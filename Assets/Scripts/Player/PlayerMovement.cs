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

    [SerializeField] private PartyStats _stats;
    private float _turnSpeed = 360f; // Set from outside

    private Vector3 _moveDirection;
    private Quaternion _turnDirection = Quaternion.identity;

    // NOTE: used in tutorial, could also become relevant somewhere else later
    // Events
    public static event Action OnPlayerMoved;
    private bool _hasFiredMoveEvent;

    public static event Action OnPlayerTurned;
    private bool _hasFiredTurnEvent;

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

        // Smoothes movement notably
        Vector3 targetVelocity = moveDirOnPlane * _stats.MovementSpeed;
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, 0.5f);

        //if (!_hasFiredMoveEvent)
        //{
        //    _hasFiredMoveEvent = true;
        //    OnPlayerMoved?.Invoke();
        //}

        if (moveDirOnPlane.sqrMagnitude > 0.002f)
            OnPlayerMoved?.Invoke();
    }

    void Turn()
    {
        Quaternion newRotation = Quaternion.RotateTowards(
            _rb.rotation,
            _turnDirection,
            _turnSpeed * Time.fixedDeltaTime
        );

        _rb.MoveRotation(newRotation);

        //if (!_hasFiredTurnEvent)
        //{
        //    _hasFiredTurnEvent = true;
        //    OnPlayerTurned?.Invoke();
        //}
        if (Quaternion.Angle(_rb.rotation, _turnDirection) > 1f)
            OnPlayerTurned?.Invoke();
    }

    public void AdjustTurnSpeed(int amountOfCharacters)
    {
        float newTurnSpeed = Mathf.Clamp(_stats.MaxTurnSpeed - amountOfCharacters * _stats.TurnSpeedAdjustPerCharacter, _stats.MinTurnSpeed, _stats.MaxTurnSpeed);
        _turnSpeed = newTurnSpeed;
    }

    public void ToggleMovement(bool state)
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        this.enabled = state;
    }
}
