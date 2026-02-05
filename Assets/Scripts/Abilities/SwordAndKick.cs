using UnityEngine;

public class SwordAndKick : TargetedAttackAbility
{
    [SerializeField] private Vector3 _hitOffset;

    [Header("Attack")]
    [SerializeField] private GameObject _hitFx;

    [SerializeField] private float _hitRange;
    [SerializeField] private float _dmg = 5;
    [SerializeField] private float _knockBack = 400f;

    [Header("SpecialAttack")]
    [SerializeField] private GameObject _specialHitFx;
    [SerializeField] private BoxHitShape _specialHitShape;
    [SerializeField] private float _specialDmg = 12;
    [SerializeField] private float _specialKnockBack = 1200f;

    protected override void ExecuteAttack(Transform target)
    {
        base.ExecuteAttack(target);

        if (_hitRange < _range) _hitRange = _range;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _hitRange,
            _targetLayer
        );

        foreach (Collider hit in hits)
        {
            // NOTE: ViewCone equals HitCone in this case
            if (!IsInViewCone(hit.transform)) continue;

            if (hit.TryGetComponent(out BaseEnemy enemy))
            {
                enemy.TakeDmg(_dmg, _knockBack);
            }
        }

        // Visuals
        _hitFx.SetActive(true);
        _hitFx.transform.localScale = new Vector3(_hitRange, 1f, _hitRange); // TODO shape
        _hitFx.transform.localPosition = _hitOffset + new Vector3(0, 0, _hitRange * 0.5f);
    }

    protected override void ExecuteSpecialAttack(Transform target)
    {
        base.ExecuteSpecialAttack(target);

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

        // Visuals
        _specialHitFx.SetActive(true);
        _specialHitFx.transform.localScale = _specialHitShape.GetHalfExtents() * 2;
        _specialHitFx.transform.localPosition = 
            _hitOffset + Vector3.forward * _specialHitShape.range * 0.5f;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Base Attack
        Gizmos.color = Color.red;

        Vector3 left = Quaternion.Euler(0, -_angle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, _angle / 2, 0) * transform.forward;

        Gizmos.DrawRay(transform.position + _hitOffset, left * _hitRange);
        Gizmos.DrawRay(transform.position + _hitOffset, right * _hitRange);

        // Special Attack
        HitQueryGizmos.DrawBoxForward(
            transform.position + _hitOffset,
            transform.forward,
            _specialHitShape,
            Color.blue
        );
    }
}
