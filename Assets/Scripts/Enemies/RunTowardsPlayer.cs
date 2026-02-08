using UnityEngine;

public class RunTowardsPlayer : MonoBehaviour
{
    [SerializeField] private EnemyStats _stats;

    private Transform _playerTransform;
    private bool _canMove = true;

    private float _moveSpeed;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;

        _moveSpeed = _stats.MoveSpeed;
        InvokeRepeating(nameof(IncreaseSpeed), _stats.MoveSpeedIncreaseInterval, _stats.MoveSpeedIncreaseInterval);
    }

    private void Update()
    {
        // Always keep facing the player
        var dir = _playerTransform.position - transform.position;
        dir.Normalize();
        transform.rotation = Quaternion.LookRotation(dir);

        if (!_canMove) return;

        // todo why enemies stuck in floor?
        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _moveSpeed * Time.deltaTime);
    }

    public void EnableMovement(bool enable)
    {
        _canMove = enable;
    }

    public void ResetMoveSpeed()
    {
        _moveSpeed = _stats.MoveSpeed;
    }

    private void IncreaseSpeed()
    {
        _moveSpeed += _stats.MoveSpeedIncrease;
    }
}
