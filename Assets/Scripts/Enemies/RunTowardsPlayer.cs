using UnityEngine;

public class RunTowardsPlayer : MonoBehaviour
{
    [SerializeField] private EnemyStats _stats;
    [SerializeField] private GameObject _speedModel;

    private Transform _playerTransform;
    private bool _canMove = true;
    private bool _isFleeing;

    // NOTE: by default turned off for target dummy, chargin enemies also turn this off
    [SerializeField] private bool _canTurn = true;
    private float _moveSpeed;
    private bool _usingSpeedModel;
    private BaseEnemy _enemy;

    private void Start()
    {
        _enemy = GetComponent<BaseEnemy>();
        _playerTransform = GameObject.FindWithTag("Player").transform;

        _moveSpeed = _stats.MoveSpeed;
        InvokeRepeating(nameof(IncreaseSpeed), _stats.MoveSpeedIncreaseInterval, _stats.MoveSpeedIncreaseInterval);
    }

    private void Update()
    {
        Vector3 dir = (_playerTransform.position - transform.position).normalized;
        if (_canTurn)
        {
            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * _stats.TurnSpeed);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(dir),
                _stats.TurnSpeed * Time.deltaTime
            );
        }

        if (!_canMove) return;

        // Ranged enemies need to turn towards the player before moving
        bool allowedToMove = true;
        if (_enemy.AttackType == AttackType.Ranged)
            allowedToMove = Vector3.Angle(transform.forward, dir) < 5;

        if (allowedToMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _moveSpeed * Time.deltaTime);
        }
    }

    public void EnableMovement(bool enable)
    {
        _canMove = enable;
    }

    public void EnableTurning(bool state)
    {
        _canTurn = state;
    }

    public void ResetMoveSpeed()
    {
        // Do not reset while the enemy is walking away
        if (_isFleeing) return;

        _moveSpeed = _stats.MoveSpeed;

        ToggleSpeedModel(false);
    }

    private void IncreaseSpeed()
    {
        _moveSpeed += _stats.MoveSpeedIncrease;

        // NOTE: test with speed model, changing enemies to high speed
        if (_speedModel == null || _usingSpeedModel) return;

        if (_moveSpeed > _enemy.Stats.MoveSpeed * 1.5f)
        {
            _moveSpeed *= 2;
            ToggleSpeedModel(true);
        }
    }

    public void Flee()
    {
        _isFleeing = true;
        _moveSpeed *= -1;
    }

    private void ToggleSpeedModel(bool state)
    {
        if (_usingSpeedModel == state) return;

        if (_speedModel == null) return;

        if (state)
            _enemy.SetBaseModel(_speedModel);
        else
            _enemy.SetBaseModel(null);

        _usingSpeedModel = state;
    }
}
