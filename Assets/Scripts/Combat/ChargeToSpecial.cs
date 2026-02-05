using UnityEngine;

public class ChargeToSpecial : SpecialResource
{
    public override bool HasSpecialAttack => true;

    [SerializeField] private int _requiredCharges = 2;
    private int _charges;

    public override bool CanUseNormalAttack => true;
    public override bool CanUseSpecialAttack => _charges >= _requiredCharges;
    public override bool LocksAttacking => false;

    public override void OnNormalAttack()
    {
        _charges++;
        if (_charges == _requiredCharges) NotifyStateChanged();
    }

    public override void OnSpecialAttack()
    {
        _charges = 0;
        NotifyStateChanged();
    }
}
