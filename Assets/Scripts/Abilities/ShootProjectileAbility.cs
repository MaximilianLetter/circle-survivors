using UnityEngine;

public class ShootProjectileAbility : TargetedAttackAbility
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileSpawn;

    [SerializeField] private int _dmg = 5;
    [SerializeField] private float _knockBack = 100f;

    protected override void ExecuteAttack(Transform target)
    {
        base.ExecuteAttack(target);

        Vector3 dir = (target.position - _projectileSpawn.position).normalized;

        // Projectiles are supposed to go straight, in order to hit potentially multiple targets
        dir.y = 0;
        dir.Normalize();

        var projectile = Instantiate(_projectilePrefab, _projectileSpawn.position, Quaternion.LookRotation(dir));
        projectile.GetComponent<BaseProjectile>().SetValues(_dmg, _knockBack);
    }
}
