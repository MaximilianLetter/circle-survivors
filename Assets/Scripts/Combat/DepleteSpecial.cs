using System.Collections;
using UnityEngine;

public class DepleteSpecial : SpecialResource
{
    [SerializeField] private int _totalAmmo = 5;
    [SerializeField] private float _reloadTime = 2f;

    private int _ammo;
    private bool _reloading = false;

    public override bool HasSpecialAttack => false;
    public override bool CanUseSpecialAttack => false;
    public override bool CanUseNormalAttack => !_reloading && _ammo > 0;
    public override bool LocksAttacking => _reloading;

    private void Start()
    {
        _ammo = _totalAmmo;
    }

    public override void OnNormalAttack()
    {
        _ammo--;
        if (_ammo <= 0 )
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        // NOTE: when special has an indicator - 
        // this could visually add one munition after another
        _reloading = true;
        NotifyStateChanged();

        yield return new WaitForSeconds(_reloadTime);

        _ammo = _totalAmmo;
        _reloading = false;
        NotifyStateChanged();
    }

    public override void OnSpecialAttack() { }
}
