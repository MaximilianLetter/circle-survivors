using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    [SerializeField] private float _totalHp = 12f;
    [SerializeField] private float _invincibleTimeframe = 0.25f;

    private float _hp;
    private bool _alive = true;
    private float _lastHitTime;

    private DeathFall _deathFall;
    private CircleOfCharacters _circle;
    private HealthPointsIndicator _healthPointsIndicator;

    private void Awake()
    {
        _circle = FindFirstObjectByType<CircleOfCharacters>();
        _deathFall = GetComponent<DeathFall>();
        _healthPointsIndicator = GetComponent<HealthPointsIndicator>();

        _hp = _totalHp;
    }

    public void TakeDmg(float dmg)
    {
        _hp -= dmg;
        SetHealthVisuals();

        _lastHitTime = Time.time;

        SoundManager.PlaySound(SoundType.CHARACTER_GET_HIT);

        if (_hp <= 0)
        {
            Die();
        }
    }

    public void RestoreHp()
    {
        _hp = _totalHp;
        SetHealthVisuals();
    }

    private void SetHealthVisuals()
    {
        float dmgTaken = 1 - (_hp / _totalHp);
        _healthPointsIndicator.SetHealthVisuals(dmgTaken);
    }

    private void Die()
    {
        _circle.RemoveCharacter(gameObject);
        _alive = false;

        DisableAbilities();
        _deathFall.PlayDeathAnimation();
    }

    private void DisableAbilities()
    {
        foreach (var ability in GetComponents<CharacterAbility>())
            ability.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!_alive) return;

        // NOTE: avoid double hits, could also be removed later on
        if (_lastHitTime + _invincibleTimeframe > Time.time) return;

        if (other.CompareTag("Enemy"))
        {
            // If character is hit, do not deal dmg but knock enemy back a bit
            var enemy = other.GetComponent<BaseEnemy>();
            TakeDmg(enemy.GetDmgStat());
            enemy.TakeDmg(0, 600);
        }

        if (other.CompareTag("EnemyProjectile"))
        {
            var projectile = other.GetComponent<EnemyProjectile>();
            TakeDmg(projectile.GetDmgStat());

            Destroy(projectile.gameObject);
        }
    }
}
