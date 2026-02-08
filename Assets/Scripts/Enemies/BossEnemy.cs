using System;
using System.Collections;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    // NOTE: restructure this in special abilities or something
    [SerializeField] private float[] _phaseThresholds = { 0.75f, 0.5f, 0.25f };
    [SerializeField] private float _retreatDuration = 2f;
    [SerializeField] private float _retreatDistance = 4f;

    private HealthPointsIndicator _healthPointsIndicator;
    private SpawnAdds _spawnAdds;

    public static event Action OnBossDefeated;

    private int _currentPhase = 0;
    //private bool _specialInProgress;

    protected override void Awake()
    {
        base.Awake();

        _healthPointsIndicator = GetComponent<HealthPointsIndicator>();
        _spawnAdds = GetComponent<SpawnAdds>();
    }

    protected override void OnDamageTaken(float incomingDmg, float knockBack)
    {
        base.OnDamageTaken(incomingDmg, knockBack);

        float hpPercent = CurrentHP / MaxHP;
        _healthPointsIndicator.SetHealthVisuals(1f - hpPercent);

        if (_currentPhase < _phaseThresholds.Length &&
            hpPercent <= _phaseThresholds[_currentPhase])
        {
            StartCoroutine(SpecialAttackSequence());

            _currentPhase++;
        }
    }

    private IEnumerator SpecialAttackSequence()
    {
        //_specialInProgress = true;
        _movement.EnableMovement(false);

        if (_stats.SpecialAbilitySound != SoundType.NONE) SoundManager.PlaySound(_stats.SpecialAbilitySound);

        yield return StartCoroutine(Retreat());

        _movement.EnableMovement(true);
        //_specialInProgress = false;
    }

    private IEnumerator Retreat()
    {
        float elapsed = 0f;
        Vector3 retreatDir = -transform.forward;

        while (elapsed < _retreatDuration)
        {
            transform.position += retreatDir * _retreatDistance * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    protected override void Die()
    {
        base.Die();

        _movement.EnableMovement(false);
        OnBossDefeated?.Invoke();
        _spawnAdds.StopAllCoroutines();
        StopAllCoroutines();
    }
}
