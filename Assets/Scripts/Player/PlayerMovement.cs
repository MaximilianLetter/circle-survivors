using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GroupDirectionIndicator _dirIndicator;

    [SerializeField] private PartyStats _stats;
    private float _turnSpeed = 360f;    // Set from outside
    private float _moveSpeed = 4f;      // Changes between in-fight or idle

    // Control variables
    private PlayerInput _playerInput;

    private Vector2 _moveInputValue;
    private Vector2 _lookInputValue;

    private bool _isGamepad;

    // Internal variables for movement
    private Quaternion _turnDirection = Quaternion.identity;

    // NOTE: used in tutorial, could also become relevant somewhere else later
    // Events
    public static event Action OnPlayerMoved;

    public static event Action OnPlayerTurned;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        _playerInput.onControlsChanged += OnControlsChanged;

        // Check once at start
        OnControlsChanged(_playerInput);
    }

    private void OnDisable()
    {
        _playerInput.onControlsChanged -= OnControlsChanged;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        _isGamepad = input.currentControlScheme == "Gamepad";

        //if (_isGamepad)
        //    ShowGamepadUIHints();
        //else
        //    ShowKeyboardUIHints();
    }

    private void Update()
    {
        _moveInputValue = _playerInput.actions["Move"].ReadValue<Vector2>();
        _lookInputValue = _playerInput.actions["Look"].ReadValue<Vector2>();

        HandleRotation();
    }

    private void HandleRotation()
    {
        Vector3 dir = Vector3.zero;

        if (_isGamepad)
        {
            // NOTE: easiest variant, maybe turning should instead use only the
            // "path" of the input value, not the direction of it
            if (_lookInputValue.sqrMagnitude > 0.2f)
            {
                dir = new Vector3(_lookInputValue.x, 0, _lookInputValue.y).ToIso();
            }
        }
        else
        {
            Ray ray = _cam.ScreenPointToRay(_lookInputValue);

            if (Physics.Raycast(ray, out RaycastHit hit, 100, _groundLayer))
            {
                dir = hit.point - transform.position;
                dir.y = 0f;
            }
        }

        if (dir != Vector3.zero)
            _turnDirection = Quaternion.LookRotation(dir);
    }

    private void FixedUpdate()
    {
        Move();
        Turn();
    }

    void Move()
    {
        Vector3 moveDirOnPlane = new Vector3(_moveInputValue.x, 0, _moveInputValue.y).ToIso();

        // Smoothes movement notably
        Vector3 targetVelocity = moveDirOnPlane * _moveSpeed;
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, 0.5f);

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

        if (Quaternion.Angle(_rb.rotation, _turnDirection) > 1f)
            OnPlayerTurned?.Invoke();
    }

    public void AdjustTurnSpeed(int amountOfCharacters)
    {
        float newTurnSpeed = Mathf.Clamp(_stats.MaxTurnSpeed - amountOfCharacters * _stats.TurnSpeedAdjustPerCharacter, _stats.MinTurnSpeed, _stats.MaxTurnSpeed);
        _turnSpeed = newTurnSpeed;
    }

    public void SetMoveSpeed(bool fightState)
    {
        _moveSpeed = fightState ? _stats.MovementSpeed : _stats.MovementSpeedIdle;
    }

    public void ToggleMovement(bool state)
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _dirIndicator.ToggleVisibility(state);

        this.enabled = state;
    }
}
