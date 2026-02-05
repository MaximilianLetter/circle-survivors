using System;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField] private float _baseHP = 18f;
    [SerializeField] private float _dmg = 6f;
    [SerializeField] private SoundType _getHitSound;
    [SerializeField] private SoundType _deathSound;

    private float _currentHP;
    private Rigidbody _rb;
    private KnockBackEnvironmentInteraction _knockBackEnvironment;

    private void Start()
    {
        _currentHP = _baseHP;

        _rb = GetComponent<Rigidbody>();
        _knockBackEnvironment = GetComponent<KnockBackEnvironmentInteraction>();
    }

    public void TakeDmg(float incomingDmg, float knockBack)
    {
        _currentHP -= incomingDmg;
        _rb.AddForce(-transform.forward * knockBack);

        if (_currentHP <= 0)
        {
            DeathSequence();
        } else
        {
            _knockBackEnvironment.CheckInteractionEnable(knockBack);
            if (_getHitSound != SoundType.NONE) SoundManager.PlaySound(_getHitSound);
        }
    }

    public float GetDmgStat()
    {
        return _dmg;
    }

    public void DeathSequence()
    {
        if (_deathSound != SoundType.NONE) SoundManager.PlaySound(_deathSound);

        Collider collider = transform.GetComponent<Collider>();
        collider.enabled = false;

        // Remove enemy layer so that it does not become a target while dying
        gameObject.layer = 0;
        Destroy(gameObject, 2);
    }
}
