using UnityEngine;

public class ShootProjectileAbility : TargetedAttackAbility
{
    [SerializeField] private Transform _projectileSpawn;
    [SerializeField] private RangedAttackStats _attackStats;

    protected override void ExecuteAttack(Transform target)
    {
        base.ExecuteAttack(target);

        Vector3 dir = (target.position - _projectileSpawn.position).normalized;

        // Projectiles are supposed to go straight, in order to hit potentially multiple targets
        dir.y = 0;
        dir.Normalize();

        var projectile = Instantiate(
            _attackStats.ProjectilePrefab,
            _projectileSpawn.position,
            Quaternion.LookRotation(dir)
        );

        float dmg = _character.ResolveStat(StatType.Damage, _attackStats.Damage);
        float knockback = _character.ResolveStat(StatType.Knockback, _attackStats.KnockBack);

        projectile
            .GetComponent<BaseProjectile>()
            .SetValues(dmg, knockback);
    }
}
