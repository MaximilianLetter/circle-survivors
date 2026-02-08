using System;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField] protected EnemyStats _stats;

    public static event Action<BaseEnemy> OnEnemyDied;

    protected float CurrentHP => _currentHP;
    protected float MaxHP => _stats.BaseHp;

    private float _currentHP;
    private Rigidbody _rb;
    private KnockBackEnvironmentInteraction _knockBackEnvironment;
    protected RunTowardsPlayer _movement;

    protected virtual void Awake()
    {
        _currentHP = _stats.BaseHp;

        _rb = GetComponent<Rigidbody>();
        _knockBackEnvironment = GetComponent<KnockBackEnvironmentInteraction>();
        _movement = GetComponent<RunTowardsPlayer>();
    }

    public void TakeDmg(float incomingDmg, float knockBack)
    {
        _currentHP -= incomingDmg;
        _rb.AddForce(-transform.forward * knockBack);

        if (_currentHP <= 0)
        {
            DeathSequence();
        } else
        {
            OnDamageTaken(incomingDmg, knockBack);
        }
    }

    protected virtual void OnDamageTaken(float amount, float knockBack)
    {
        _knockBackEnvironment.CheckInteractionEnable(knockBack);
        if (_stats.GetHitSound != SoundType.NONE) SoundManager.PlaySound(_stats.GetHitSound);

        _movement.ResetMoveSpeed();
    }

    public float GetDmgStat()
    {
        return _stats.Damage;
    }

    public void DeathSequence()
    {
        if (_stats.DeathSound != SoundType.NONE) SoundManager.PlaySound(_stats.DeathSound);

        Collider collider = transform.GetComponent<Collider>();
        collider.enabled = false;

        Die();
    }

    protected virtual void Die()
    {
        OnEnemyDied?.Invoke(this);

        // Remove enemy layer so that it does not become a target while dying
        gameObject.layer = 0;
        Destroy(gameObject, 2);
    }
}
