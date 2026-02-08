using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private float _timeToLive = 5;
    [SerializeField] private SoundType _sound;

    private float _dmg;
    private float _knockBack;

    private HashSet<BaseEnemy> _hitEnemies = new();

    protected virtual void Start()
    {
        Destroy(gameObject, _timeToLive);
    }

    protected virtual void Update()
    {
        transform.position += _speed * Time.deltaTime * transform.forward;
    }

    public void SetValues(float dmg, float knockBack)
    {
        _dmg = dmg;
        _knockBack = knockBack;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // NOTE: could also be destroyed when hitting obstacles
        if (!other.CompareTag("Enemy")) return;

        BaseEnemy enemy = other.GetComponent<BaseEnemy>();
        if (enemy == null || _hitEnemies.Contains(enemy)) return;

        // Avoids double hitting the same enemy
        _hitEnemies.Add(enemy);

        enemy.TakeDmg(_dmg, _knockBack);
        SoundManager.PlaySound(_sound);

        OnEnemyHit(enemy);
    }

    protected abstract void OnEnemyHit(BaseEnemy enemy);
}
