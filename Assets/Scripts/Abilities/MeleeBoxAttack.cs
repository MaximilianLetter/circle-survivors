using UnityEngine;

public class MeleeBoxAttack : TargetedAttackAbility
{
    [Header("Attack")]
    [SerializeField] private Vector3 _hitOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private GameObject _hitFx;
    [SerializeField] private GameObject _specialHitFx;
    [SerializeField] private MeleeAttackStats _attackStats;

    protected override void ExecuteAttack(Transform target)
    {
        base.ExecuteAttack(target);

        PerformAttack(
            _attackStats.Damage,
            _attackStats.KnockBack,
            _attackStats.HitShape,
            _hitFx
        );
    }

    protected override void ExecuteSpecialAttack(Transform target)
    {
        base.ExecuteSpecialAttack(target);

        PerformAttack(
            _attackStats.SpecialDamage,
            _attackStats.SpecialKnockBack,
            _attackStats.SpecialHitShape,
            _specialHitFx
        );
    }

    private void PerformAttack(float baseDamage, float baseKnockback, BoxHitShape shape, GameObject hitFx)
    {
        Collider[] hits = HitQuery.BoxForward(
            transform.position + _hitOffset,
            transform.forward,
            shape,
            _targetLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out BaseEnemy enemy))
            {
                // DAMAGE
                float dmg = _character.ResolveStat(
                    StatType.Damage,
                    baseDamage
                );

                // KNOCKBACK
                float knockBack = _character.ResolveStat(
                    StatType.Knockback,
                    baseKnockback
                );

                // Apply values
                enemy.TakeDmg(dmg, knockBack);
            }

            hitFx.SetActive(true);
            hitFx.transform.localScale = shape.GetHalfExtents() * 2;
            hitFx.transform.localPosition =
                _hitOffset + Vector3.forward * shape.range * 0.5f;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Normal Attack
        HitQueryGizmos.DrawBoxForward(
            transform.position + _hitOffset,
            transform.forward,
            _attackStats.HitShape,
            Color.red
        );

        // Special Attack
        HitQueryGizmos.DrawBoxForward(
            transform.position + _hitOffset,
            transform.forward,
            _attackStats.SpecialHitShape,
            Color.blue
        );
    }
}
