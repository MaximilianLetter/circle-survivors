using System;
using UnityEngine;

public abstract class SpecialResource : MonoBehaviour
{
    public event Action OnStateChanged;

    protected void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    public abstract bool HasSpecialAttack { get; }
    public abstract bool CanUseNormalAttack {  get; }
    public abstract bool CanUseSpecialAttack { get; }
    public abstract bool LocksAttacking { get; }

    public abstract void OnNormalAttack();
    public abstract void OnSpecialAttack();
}
