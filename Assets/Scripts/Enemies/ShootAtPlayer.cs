using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ShootAtPlayer : MonoBehaviour
{
    [SerializeField] private RangedEnemyStats _stats;
    [SerializeField] private Transform _projectileSpawn;
    [SerializeField] private Animator _animator;

    private RunTowardsPlayer _movement;
    private Transform _playerTransform;

    private bool _isShooting = false;
    private bool _isMoving = false;

    private void Start()
    {
        _movement = GetComponent<RunTowardsPlayer>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (_isShooting) return;

        float distToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distToPlayer <= _stats.RangedAttackRange)
        {
            _movement.EnableMovement(false);
            StartCoroutine(Shoot());
        }
        else
        {
            if (!_isMoving)
            {
                _movement.EnableMovement(true);
                _isMoving = true;
            }
        }
    }

    private IEnumerator Shoot()
    {
        _isShooting = true;
        _isMoving = false;

        _animator.SetTrigger("Attack");
        SoundManager.PlaySound(_stats.RangedAttackSound);

        Vector3 dir = transform.forward;
        GameObject projectile = Instantiate(_stats.Projectile, _projectileSpawn.position, Quaternion.LookRotation(dir));
        projectile.GetComponent<EnemyProjectile>().SetValues(_stats.RangedAttackDamage);

        yield return new WaitForSeconds(_stats.RangedAttackCooldown);

        _isShooting = false;
    }
}
