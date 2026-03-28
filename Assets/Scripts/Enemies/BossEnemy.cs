using System;
using System.Collections;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    // NOTE: restructure this in special abilities or something
    [SerializeField] private float[] _phaseThresholds = { 0.75f, 0.5f, 0.25f };

    private HealthPointsIndicator _healthPointsIndicator;

    private BossAbility[] _abilities;

    public static event Action OnBossDefeated;

    private int _currentPhase = 0;
    //private bool _specialInProgress;

    protected override void Awake()
    {
        base.Awake();

        _healthPointsIndicator = GetComponent<HealthPointsIndicator>();
        //_spawnAdds = GetComponent<SpawnAdds>();

        _abilities = GetComponents<BossAbility>();
    }

    private void Start()
    {
        // Start all regular returning boss effects (like spawning adds)
        foreach (var ability in _abilities)
        {
            if (ability.HasIntervalEffect)
            {
                ability.StartAbilityRoutine();
            }   
        }
    }

    protected override void OnDamageTaken(float incomingDmg, float knockBack)
    {
        base.OnDamageTaken(incomingDmg, knockBack);

        float hpPercent = CurrentHP / MaxHP;
        _healthPointsIndicator.SetHealthVisuals(1f - hpPercent);

        if (_currentPhase < _phaseThresholds.Length &&
            hpPercent <= _phaseThresholds[_currentPhase])
        {
            foreach (var ability in _abilities)
            {
                if (ability.HasTriggerEffect)
                {
                    if (ability.HasMoveEffect)
                    {
                        StartCoroutine(AbilityMovement(ability));
                    }
                    else
                        ability.FireAbility();
                }
            }

            string catchPhrase = Stats.CatchPhrase;
            if (catchPhrase != null && catchPhrase != "")
                WorldTextManager.Instance.ShowWorldText(catchPhrase, transform.position, false);

            _currentPhase++;
        }
    }

    private IEnumerator AbilityMovement(BossAbility ability)
    {
        _movement.EnableMovement(false);
        _movement.EnableTurning(false);

        yield return StartCoroutine(ability.RunMovementRoutine());

        _movement.EnableMovement(true);
        _movement.EnableTurning(true);
    }

    protected override void Die()
    {
        base.Die();

        foreach (var ability in _abilities)
        {
            ability.StopAllCoroutines();
        }
        StopAllCoroutines();

        _movement.EnableMovement(false);
        OnBossDefeated?.Invoke();
    }
}
