using UnityEngine;
using UnityEngine.InputSystem;

public class MenuMovement : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Rigidbody _rb;

    [Header("CameraTargetOffset")]
    [SerializeField] private float _offsetScale = 0.075f;
    [SerializeField] private Transform _camTarget;

    private PlayerInput _playerInput;

    private Vector2 _moveInputValue;
    private Vector2 _lookInputValue;

    private Vector3 _moveDirection;

    private bool _isGamepad;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        // Set to center of screen in beginning
        //if (_playerInput.currentControlScheme == "Keyboard&Mouse")
        //{
        //    Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        //    Mouse.current.WarpCursorPosition(screenCenter);
        //}
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

        HandleMoveInput();
    }

    private void HandleMoveInput()
    {
        if (_isGamepad)
        {
            _moveDirection = new Vector3(_moveInputValue.x, 0, _moveInputValue.y).ToIso();
        }
        else
        {
            Ray ray = _cam.ScreenPointToRay(_lookInputValue);

            if (Physics.Raycast(ray, out RaycastHit hit, 100, _groundLayer))
            {
                Vector3 dir = hit.point - transform.position;
                dir.y = 0f;

                _moveDirection = dir;
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float factor = _isGamepad ? _moveSpeed * 2.5f : _moveSpeed;
        Vector3 targetVelocity = _moveDirection * factor;
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, 0.5f);

        // Move relative cam target for camera movement
        if (_camTarget != null)
        {
            _camTarget.position = new Vector3(
                transform.position.x * _offsetScale,
                0,
                transform.position.z * _offsetScale
            );
        }
    }
}
