using System;
using UnityEngine;

public abstract class SpecialResource : MonoBehaviour
{
    [SerializeField] protected SpecialResourceIndicator _indicator;

    public event Action OnStateChanged;

    protected void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    private void Awake()
    {
        _character = GetComponent<BaseCharacter>();
    }

    public abstract bool HasSpecialAttack { get; }
    public abstract bool CanUseNormalAttack {  get; }
    public abstract bool CanUseSpecialAttack { get; }
    public abstract bool LocksAttacking { get; }

    public abstract void OnNormalAttack();
    public abstract void OnSpecialAttack();

    protected BaseCharacter _character;
}
