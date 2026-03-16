using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossEnemy))]
public abstract class BossAbility : MonoBehaviour
{
    public bool HasIntervalEffect => _hasIntervalEffect;
    [SerializeField] private bool _hasIntervalEffect;

    public bool HasTriggerEffect => _hasTriggerEffect;
    [SerializeField] protected bool _hasTriggerEffect;

    public bool HasMoveEffect => _hasMoveEffect;
    [SerializeField] private bool _hasMoveEffect;

    protected Coroutine _intervalRoutine;
    protected BaseEnemy _baseEnemy;

    private void Start()
    {
        _baseEnemy = GetComponent<BaseEnemy>();
    }

    public abstract void FireAbility();

    public void StartAbilityRoutine()
    {
        _intervalRoutine = StartCoroutine(RunAbilityRoutine());
    }

    protected abstract IEnumerator RunAbilityRoutine();

    public abstract IEnumerator RunMovementRoutine();
}
