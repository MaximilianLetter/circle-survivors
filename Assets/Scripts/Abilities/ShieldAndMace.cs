using UnityEngine;

public class ShieldAndMace : TargetedAttackAbility
{
    [SerializeField] private Vector3 _hitOffset = new Vector3(0f, 1f, 0f);

    [Header("Attack")]
    [SerializeField] private GameObject _hitFx;

    [SerializeField] private BoxHitShape _hitShape;
    [SerializeField] private float _dmg = 6f;
    [SerializeField] private float _knockBack = 200f;

    [Header("SpecialAttack")]
    [SerializeField] private GameObject _specialHitFx;

    [SerializeField] private BoxHitShape _specialHitShape;
    [SerializeField] private float _specialDmg = 12f;
    [SerializeField] private float _specialKnockBack = 1200f;

    protected override void ExecuteAttack(Transform target)
    {
        base.ExecuteAttack(target);

        Collider[] hits = HitQuery.BoxForward(
            transform.position + _hitOffset,
            transform.forward,
            _hitShape,
            _targetLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out BaseEnemy enemy))
            {
                enemy.TakeDmg(_dmg, _knockBack);
            }
        }

        // Visuals
        _hitFx.SetActive(true);
        _hitFx.transform.localScale = _hitShape.GetHalfExtents() * 2;
        _hitFx.transform.localPosition =
            _hitOffset + Vector3.forward * _hitShape.range * 0.5f;
    }

    protected override void ExecuteSpecialAttack(Transform target)
    {
        base.ExecuteSpecialAttack(target);

        // NOTE: this is exactly the same logic as above
        // -> restructure at some point
        Collider[] hits = HitQuery.BoxForward(
            transform.position + _hitOffset,
            transform.forward,
            _specialHitShape,
            _targetLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out BaseEnemy enemy))
            {
                enemy.TakeDmg(_specialDmg, _specialKnockBack);
            }
        }

        // TODO visuals not fully line up
        // Visuals
        _specialHitFx.SetActive(true);
        _specialHitFx.transform.localScale = _specialHitShape.GetHalfExtents() * 2;
        _specialHitFx.transform.localPosition = 
            _hitOffset + Vector3.forward * _specialHitShape.range * 0.5f;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Normal Attack
        HitQueryGizmos.DrawBoxForward(
            transform.position + _hitOffset,
            transform.forward,
            _hitShape,
            Color.red
        );

        // Special Attack
        HitQueryGizmos.DrawBoxForward(
            transform.position + _hitOffset,
            transform.forward,
            _specialHitShape,
            Color.blue
        );
    }
}
