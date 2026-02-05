using System;
using UnityEngine;

[RequireComponent (typeof(SpecialResource))]
public class TargetedAttackAbility : CharacterAbility
{
    [SerializeField] private GameObject _baseModel;
    [SerializeField] private GameObject _specialModel;
    
    [Header("Targeting")]
    [SerializeField] protected float _range = 12f;
    [SerializeField] protected float _angle = 90f;
    [SerializeField] protected LayerMask _targetLayer;
    [SerializeField] protected LayerMask _obstacleMask;

    [Header("Attacking")]
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private SoundType _attackSound;
    [SerializeField] private SoundType _specialAttackSound;

    [Header("Turning")]
    [SerializeField] private float _turnSpeed = 180f;
    [SerializeField] private float _returnDelay = 0.5f;

    protected enum SpecialState
    {
        Idle,       // Firing normal attacks
        Ready,      // Special is ready, visual is showing
        Executing,  // Special is/was performed
        Locked      // Cannot attack (reload)
    }

    protected SpecialState _specialState = SpecialState.Idle;
    private SpecialResource _specialResource;

    private Transform _currentTarget;
    private float _lastFire;
    private float _returnTime;
    private bool _returningToBase;

    private bool _visualSpecialActive;

    protected override void Awake()
    {
        base.Awake();

        _specialResource = GetComponent<SpecialResource>();
        _specialResource.OnStateChanged += OnSpecialResourceChanged;

        UpdateBaseRotation();
        ResolveSpecialState();
        ApplyVisualState();
    }

    private void OnDestroy()
    {
        if (_specialResource != null)
            _specialResource.OnStateChanged -= OnSpecialResourceChanged;
    }

    private void Update()
    {
        if (_specialState == SpecialState.Locked)
        {
            TryReturnToBaseRotation();
            return;
        }

        if (IsOnCooldown())
        {
            TryReturnToBaseRotation();
            return;
        }

        CheckForTargetAndAttack();
    }

    // -------------------------
    // Targeting & Attacking
    // -------------------------

    private void CheckForTargetAndAttack()
    {
        // Look if new target in range, cone, and sight
        if (_currentTarget == null)
            _currentTarget = FindTarget();

        if (_currentTarget == null)
        {
            RotateTowardsBaseRotation();
            return;
        }

        // If target is found, turn towards it
        if (RotateTowardsTarget(_currentTarget))
        {
            // Actual attack triggering point
            if (_specialResource.HasSpecialAttack &&
                _specialState == SpecialState.Ready)
            {
                ExecuteSpecialAttack(_currentTarget);
            }
            else if (_specialResource.CanUseNormalAttack)
            {
                ExecuteAttack(_currentTarget);
            }

            _currentTarget = null;
        }
    }

    private Transform FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _range,
            _targetLayer
        );

        foreach (Collider hit in hits )
        {
            if (!IsInViewCone(hit.transform)) continue;
            if (!HasLineOfSight(hit.transform)) continue;

            return hit.transform;
        }

        return null;
    }

    protected bool IsInViewCone(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;

        float angle = Vector3.Angle(transform.forward, dirToTarget);

        return angle < _angle * 0.5f;
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position;
        Vector3 targetPos = target.position;

        Vector3 dir = targetPos - origin;

        Physics.Raycast(origin, dir.normalized, out RaycastHit hit, _range, _obstacleMask | _targetLayer);

        return hit.transform == target;
    }

    // -------------------------
    // Attacks
    // -------------------------

    /// <summary>
    /// Executes the actual attack, putting everything on cooldown.
    /// This is overwritten by the actual attack abilities.
    /// </summary>
    /// <param name="target">Target to execute attack towards.</param>
    protected virtual void ExecuteAttack(Transform target)
    {
        StartCooldown();
        _specialResource.OnNormalAttack();
        _returningToBase = true;

        _animator.SetTrigger("Attack");
        SoundManager.PlaySound(_attackSound);
    }

    protected virtual void ExecuteSpecialAttack(Transform target)
    {
        StartCooldown();
        _specialState = SpecialState.Executing;
        _specialResource.OnSpecialAttack();
        _returningToBase = true;

        _animator.SetTrigger("SpecialAttack");
        SoundManager.PlaySound(_specialAttackSound);
    }

    // -------------------------
    // Cooldown & Rotation
    // -------------------------

    private bool IsOnCooldown()
    {
        return Time.time < _lastFire + _cooldown;
    }

    private void StartCooldown()
    {
        _lastFire = Time.time;
        _returnTime = Time.time + _returnDelay;
    }

    private bool RotateTowardsTarget(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return true;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);

        float angle = Quaternion.Angle(transform.rotation, targetRot);
        return angle < 2f;
    }

    private void TryReturnToBaseRotation()
    {
        if (Time.time > _returnTime && _currentTarget == null)
            RotateTowardsBaseRotation();
    }

    private void RotateTowardsBaseRotation()
    {
        if (!_returningToBase) return;

        if (!RotateBackAndCheckDone()) return;

        CompleteReturnToBase();
    }

    private void CompleteReturnToBase()
    {
        _returningToBase = false;
        TryApplyVisualTransition();
    }

    private bool RotateBackAndCheckDone()
    {
        if (IsAtBaseRotation()) return true;

        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, base._baseRotation, _turnSpeed * Time.deltaTime);

        return false;
    }

    private bool IsAtBaseRotation()
    {
        return Quaternion.Angle(transform.localRotation, _baseRotation) < 0.2f;
    }

    // -------------------------
    // Special State
    // -------------------------

    private void OnSpecialResourceChanged()
    {
        bool wasLocked = _specialState == SpecialState.Locked;

        ResolveSpecialState();

        if (wasLocked) TryApplyVisualTransition();
    }

    private void ResolveSpecialState()
    {
        if (_specialResource.LocksAttacking)
        {
            _specialState = SpecialState.Locked;
            return;
        }

        if (_specialResource.CanUseSpecialAttack)
        {
            _specialState = SpecialState.Ready;
            return;
        }

        _specialState = SpecialState.Idle;
    }

    // -------------------------
    // Visuals Special State
    // -------------------------

    private void TryApplyVisualTransition()
    {
        bool showSpecial =
            _specialState == SpecialState.Ready ||
            _specialState == SpecialState.Locked;

        if (_visualSpecialActive == showSpecial)
            return;

        _visualSpecialActive = showSpecial;
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        _baseModel.SetActive(!_visualSpecialActive);
        _specialModel.SetActive(_visualSpecialActive);
    }

    // -------------------------
    // Debug
    // -------------------------

    protected virtual void OnDrawGizmos()
    {
        // Draw range + angles
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _range);

        Vector3 left = Quaternion.Euler(0, -_angle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, _angle / 2, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, left * _range);
        Gizmos.DrawRay(transform.position, right * _range);
    }
}