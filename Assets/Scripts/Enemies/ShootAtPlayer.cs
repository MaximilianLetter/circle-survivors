using System.Collections;
using UnityEngine;

public class ShootAtPlayer : MonoBehaviour
{
    [SerializeField] private EnemyRangedStats _stats;
    [SerializeField] private Transform _projectileSpawn;
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _attackModel;
    [SerializeField] private GameObject _reloadModel;
    [SerializeField] private GameObject _shootVfx;
    [SerializeField] private float _shotAnimationTime = 0.5f;

    [Header("Pre Shoot Variant")]
    [SerializeField] private GameObject _preShootModel;
    [SerializeField] private float _preShootTime = 0f;

    private RunTowardsPlayer _movement;
    private BaseEnemy _baseEnemy;
    private Transform _playerTransform;

    private bool _canAttack = true;
    private bool _isShooting = false;
    private bool _isMoving = false;

    private void Start()
    {
        _movement = GetComponent<RunTowardsPlayer>();
        _baseEnemy = GetComponent<BaseEnemy>();
        _playerTransform = GameObject.FindWithTag("Player").transform;

        if (_shootVfx != null ) _shootVfx.SetActive( false );
    }

    private void Update()
    {
        if (!_canAttack) return;

        if (_isShooting) return;

        float distToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distToPlayer <= _stats.RangedAttackRange)
        {
            float angleTowardsPlayer = Vector3.Angle(transform.forward,  _playerTransform.position - transform.position);

            if (Mathf.Abs(angleTowardsPlayer) < _stats.RangedAttackPrecision)
            {
                _movement.EnableMovement(false);
                _movement.EnableTurning(false);
                StartCoroutine(Shoot());

                return;
            }
        }

        if (!_isMoving)
        {
            _movement.EnableMovement(true);
            _movement.EnableTurning(true);
            _isMoving = true;
        }
    }

    private IEnumerator Shoot()
    {
        _isShooting = true;
        _isMoving = false;

        if (_preShootTime != 0)
        {
            _baseEnemy.SetBaseModel(_preShootModel, true);
            yield return new WaitForSeconds(_preShootTime);
        }

        _animator.SetTrigger("Attack");
        SoundManager.PlaySound(_baseEnemy.Audio.Attack);
        if (_shootVfx != null) _shootVfx.SetActive(true);

        Vector3 dir = transform.forward;
        GameObject projectile = Instantiate(_stats.Projectile, _projectileSpawn.position, Quaternion.LookRotation(dir));
        projectile.GetComponent<EnemyProjectile>().SetValues(_stats.RangedAttackDamage);

        if (_attackModel != null)
            _baseEnemy.SetBaseModel(_attackModel, true);

        yield return new WaitForSeconds(_shotAnimationTime);

        if (_reloadModel != null)
            _baseEnemy.SetBaseModel(_reloadModel, true);

        yield return new WaitForSeconds(_stats.RangedAttackCooldown - _shotAnimationTime);

        _isShooting = false;
        _baseEnemy.SetBaseModel(null);
    }

    public void DeactivateAbility()
    {
        _canAttack = false;

        StopAllCoroutines();
    }
}
