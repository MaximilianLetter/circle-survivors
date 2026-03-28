using System;
using UnityEngine;

public enum ModelVariant
{
    Idle,
    Fight,
    Special
}

[RequireComponent(typeof(LocalModifierSystem))]
[RequireComponent(typeof(HealthPointsIndicator))]
public class BaseCharacter : MonoBehaviour, IStatContext
{
    // Classification of object
    public Faction Faction => Faction.Player;
    public AttackType AttackType => _defaultAttackType;
    public CharacterType CharacterType => _characterType;

    [SerializeField] private AttackType _defaultAttackType;
    [SerializeField] private CharacterType _characterType;

    public CharacterStats Stats => _stats;
    [SerializeField] private CharacterStats _stats;

    public CharacterAudioProfile Audio => _audio;
    [SerializeField] private CharacterAudioProfile _audio;


    // NOTE: this is a bit doubled with TargetedAttackAbility
    // maybe remove from there to here
    [SerializeField] private GameObject _idleModel;
    [SerializeField] private GameObject _fightModel;
    [SerializeField] private GameObject _specialModel;

    public LocalModifierSystem LocalModifierSystem => _localModifierSystem;
    private LocalModifierSystem _localModifierSystem;

    private ModifierIndicator _modifierIndicator;

    private float _hp;
    private float HP
    {
        get { return _hp; }
        set
        {
            _hp = Mathf.Clamp(value, 0f, _maxHp);
            
            if (_isInitialized) SetHealthVisuals();
        }
    }
    private float _maxHp;

    private bool _alive = true;
    private float _lastHitTime;

    private bool NewlyCreated => Time.time < _creationTime + 1f;
    private float _creationTime;
    private bool _isInitialized;

    private DeathFall _deathFall;
    private PartyOfCharacters _party;
    private HealthPointsIndicator _healthPointsIndicator;
    private SpecialResource _specialResource;
    private BlockProjectiles _blockProjectiles;

    // Events
    public static event Action OnCollectablePickedUp;

    private void Awake()
    {
        _creationTime = Time.time;

        _party = FindFirstObjectByType<PartyOfCharacters>();
        _deathFall = GetComponent<DeathFall>();
        _healthPointsIndicator = GetComponent<HealthPointsIndicator>();
        _localModifierSystem = GetComponent<LocalModifierSystem>();
        _modifierIndicator = GetComponent<ModifierIndicator>();
        _specialResource = GetComponent<SpecialResource>();

        if (_characterType == CharacterType.Guardian)
            _blockProjectiles = GetComponent<BlockProjectiles>();

        _maxHp = ResolveStat(StatType.MaxHp, _stats.MaxHP);
        HP = _maxHp;
    }

    private void Start()
    {
        _isInitialized = true;
        SetHealthVisuals();

        // Differentiate between characters added during wave or in between
        bool fightModel = EnemyManager.Instance.GetWaveRunningState();
        ApplyVisualState(fightModel ? ModelVariant.Fight : ModelVariant.Idle);
    }

    private void OnEnable()
    {
        _localModifierSystem.OnModifiersChanged += OnLocalModifiersChanged;
        GlobalModifierSystem.Instance.OnModifiersChanged += OnGlobalModifiersChanged;
    }

    private void OnDisable()
    {
        _localModifierSystem.OnModifiersChanged -= OnLocalModifiersChanged;
        GlobalModifierSystem.Instance.OnModifiersChanged -= OnGlobalModifiersChanged;
    }

    private void OnLocalModifiersChanged(StatType type)
    {
        _modifierIndicator.IncreaseIndicatorLevel();

        if (type == StatType.MaxHp)
            RecalculateMaxHp();
    }

    private void OnGlobalModifiersChanged(StatType type)
    {
        if (type == StatType.MaxHp)
            RecalculateMaxHp();
    }

    public void TakeDmg(float dmg)
    {
        HP -= dmg;

        _lastHitTime = Time.time;

        SoundManager.PlaySound(_audio.GetHit);

        if (HP <= 0)
        {
            Die();
        }
    }

    public void RestoreHp()
    {
        HP = _maxHp;
    }

    // NOTE: required for replacing characters in the party
    public float GetHpPercentage()
    {
        return HP / _maxHp;
    }

    public void SetHpPercentage(float percentage)
    {
        HP = _maxHp * percentage;
    }

    public void RecalculateMaxHp()
    {
        // Keep percentual missing HP
        float oldMaxHp = _maxHp;
        float hpPercent = oldMaxHp > 0 ? _hp / oldMaxHp : 1f;

        _maxHp = ResolveStat(StatType.MaxHp, _stats.MaxHP);

        SetHpPercentage(hpPercent);

        // NOTE: variant with directly adding the gained HP, does break when loving max hp again
        // float missingHP = _maxHp - HP;
        // _maxHp = ResolveStat(StatType.MaxHp, _stats.MaxHP);
        // HP = _maxHp - missingHP;
    }

    private void SetHealthVisuals()
    {
        float dmgTaken = 1 - (_hp / _maxHp);
        _healthPointsIndicator.SetHealthVisuals(dmgTaken);
    }

    public float ResolveStat(StatType type, float baseValue)
    {
        float value = baseValue;

        value = GlobalModifierSystem.Instance
            .ApplyModifiers(type, value, this);

        value = _localModifierSystem
            .ApplyModifiers(type, value, this);

        return value;
    }

    private void Die()
    {
        _party.RemoveCharacter(gameObject);
        _alive = false;

        DisableAbilities();
        _deathFall.PlayDeathAnimation();
    }

    private void DisableAbilities()
    {
        foreach (var ability in GetComponents<CharacterAbility>())
            ability.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_alive) return;

        if (other.CompareTag("Collectable"))
        {
            BaseCollectable collectable = other.GetComponent<BaseCollectable>();
            CollectableType type = collectable.GetCollectableType();

            switch (type)
            {
                case CollectableType.Character:
                    if (!TryToPickUpCharacter(collectable, other.transform.position, other.transform.rotation)) return;
                    break;

                case CollectableType.HealthPickUp:
                    if (!TryToPickUpHealth(collectable)) return;
                    break;

                // NOTE: Party Increase is basically a stat modifier, the type
                // enum is multi-used for spawning objects. Semi-clean.
                case CollectableType.PartyIncrease:
                    if (!TryToPickUpModifier(collectable)) return;
                    break;

                case CollectableType.StatModifier:
                    if (!TryToPickUpModifier(collectable)) return;
                    break;
            }

            // Pick up was successful -> Destroy Object
            collectable.DestroyCollectable();
            return;
        }

        // NOTE: avoid double hits, could also be removed later on
        if (_lastHitTime + _stats.InvincibleTimeFrame > Time.time) return;

        if (other.CompareTag("Enemy"))
        {
            // If character is hit, do not deal dmg but knock enemy back a bit
            var enemy = other.GetComponent<BaseEnemy>();
            TakeDmg(enemy.GetDmgStat());
            enemy.TakeDmg(0, 600, transform.forward);
        }

        if (other.CompareTag("EnemyProjectile"))
        {
            var projectile = other.GetComponent<EnemyProjectile>();

            if (_blockProjectiles != null)
            {
                _blockProjectiles.Block();
            }
            else
                TakeDmg(projectile.GetDmgStat());

            Destroy(projectile.gameObject);
        }
    }

    private bool TryToPickUpCharacter(BaseCollectable collectable, Vector3 pos, Quaternion rot)
    {
        CharacterType type = collectable.GetCharacterType();

        if (_party.CanAddCharacter())
        {
            _party.AddCharacter(type);
            SoundManager.PlaySound(SoundManager.Instance.Library.CollectCharacter);
        }
        else
        {
            // Try to exchange characters
            if (NewlyCreated) return false;
            if (type == _characterType) return false;

            _party.ExchangeCharacter(type, this, pos, rot);
            SoundManager.PlaySound(SoundManager.Instance.Library.CollectCharacter);
        }

        return true;
    }

    private bool TryToPickUpHealth(BaseCollectable collectable)
    {
        //_party.RestoreHpForAllCharacters();
        if (HP == _maxHp) return false;

        RestoreHp();
        SoundManager.PlaySound(SoundManager.Instance.Library.CollectPickUp); // TODO add heal sound

        return true;
    }

    private bool TryToPickUpModifier(BaseCollectable collectable)
    {
        var mod = collectable.GetStatModifier();

        if (mod.StatType == StatType.PartySize)
        {
            GlobalModifierSystem.Instance.AddModifier(mod.CreateRuntimeInstance());
        }
        else
        {
            if (mod.TargetCharacterType != CharacterType.None && _characterType != mod.TargetCharacterType)
                return false;

            _localModifierSystem.AddModifier(mod.CreateRuntimeInstance());
        }

        SoundManager.PlaySound(SoundManager.Instance.Library.CollectPickUp);
        OnCollectablePickedUp?.Invoke();

        return true;
    }

    public void ResetCharacterState()
    {
        _specialResource.ResetToDefault();

        ApplyVisualState(ModelVariant.Idle);
    }

    public void ApplyVisualState(ModelVariant model)
    {
        _idleModel.SetActive(false);
        _fightModel.SetActive(false);
        _specialModel.SetActive(false);

        switch (model)
        {
            case ModelVariant.Idle:
                _idleModel.SetActive(true);
                break;

            case ModelVariant.Fight:
                _fightModel.SetActive(true);
                break;

            case ModelVariant.Special:
                _specialModel.SetActive(true);
                break;
        }
    }
}
