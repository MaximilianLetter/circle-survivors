using System.Collections;
using UnityEngine;

public class DepleteSpecial : SpecialResource
{
    [SerializeField] private RangedAttackStats _stats;

    private int _ammo;
    private bool _reloading = false;

    public override bool HasSpecialAttack => false;
    public override bool CanUseSpecialAttack => false;
    public override bool CanUseNormalAttack => !_reloading && _ammo > 0;
    public override bool LocksAttacking => _reloading;

    // TODO: Revisit at some point, see comment in Reload() for more information.
    private float _indicatorAnimationTime;

    private int Ammo
    {
        get { return _ammo; }
        set
        {
            _ammo = Mathf.Clamp(value, 0, _stats.TotalAmmo);
            float fillPercentage = (float)_ammo / (float)_stats.TotalAmmo;

            if (fillPercentage == 1f)
            {
                _indicator.SetFillPercentage(fillPercentage, false, _indicatorAnimationTime);
            }
            else
            {
                _indicator.SetFillPercentage(fillPercentage);
            }
        }
    }

    private void Start()
    {
        Ammo = _stats.TotalAmmo;
    }

    public override void OnNormalAttack()
    {
        Ammo--;

        if (Ammo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        _reloading = true;
        NotifyStateChanged();

        // TODO: Revisit at some point.

        // NOTE: this is kinda hacky.
        // Indicator needs time after reaching 0 ammunition to visualize.
        // Show reload process after that time
        float delayTime = 0.3f; // Animation time is 0.25f for reference
        float totalReloadTime = _character.ResolveStat(StatType.ReloadTime, _stats.ReloadTime);

        _indicatorAnimationTime = totalReloadTime - delayTime;

        yield return new WaitForSeconds(delayTime);

        Ammo = _stats.TotalAmmo;

        yield return new WaitForSeconds(_indicatorAnimationTime);

        _reloading = false;
        NotifyStateChanged();
    }

    public override void OnSpecialAttack() { }
}
