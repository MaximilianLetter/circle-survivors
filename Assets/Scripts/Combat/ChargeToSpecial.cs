using UnityEngine;

public class ChargeToSpecial : SpecialResource
{
    public override bool HasSpecialAttack => true;

    [SerializeField] private int _requiredCharges = 2;
    private int _charges;

    public override bool CanUseNormalAttack => true;
    public override bool CanUseSpecialAttack => _charges >= _requiredCharges;
    public override bool LocksAttacking => false;

    private int Charges
    {
        get { return _charges; }
        set {
            _charges = Mathf.Clamp(value, 0, _requiredCharges);
            float fillPercentage = (float)_charges / (float)_requiredCharges;

            _indicator.SetFillPercentage(fillPercentage, true);
        }
    }

    private void Start()
    {
        Charges = 0;
    }

    public override void OnNormalAttack()
    {
        Charges++;
        if (_charges == _requiredCharges) NotifyStateChanged();
    }

    public override void OnSpecialAttack()
    {
        Charges = 0;
        NotifyStateChanged();
    }
}
