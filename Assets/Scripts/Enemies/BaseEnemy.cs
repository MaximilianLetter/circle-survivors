using System;
using System.Collections;
using UnityEngine;

public class BaseEnemy : MonoBehaviour, IStatContext
{
    public Faction Faction => Faction.Enemy;
    public AttackType AttackType => _attackType;
    public CharacterType CharacterType => CharacterType.None;

    [SerializeField] protected AttackType _attackType;

    public EnemyStats Stats => _stats;
    [SerializeField] private EnemyStats _stats;

    public EnemyAudioProfile Audio => _audio;
    [SerializeField] private EnemyAudioProfile _audio;

    [SerializeField] private GameObject _baseModel;
    [SerializeField] private GameObject _getHitModel;
    private GameObject _activeBaseModel;

    private Coroutine _getHitRoutine;

    public static event Action<BaseEnemy> OnEnemyDied;
    public static event Action<BaseEnemy> OnEnemyRemoved;

    protected float CurrentHP => _currentHP;
    protected float MaxHP => _stats.BaseHp;

    private float _currentHP;
    private Rigidbody _rb;
    private KnockBackEnvironmentInteraction _knockBackEnvironment;
    protected RunTowardsPlayer _movement;
    protected DefensiveStance _defensiveStance;

    private bool _overridesHitPose;

    protected virtual void Awake()
    {
        _currentHP = _stats.BaseHp;

        _rb = GetComponent<Rigidbody>();
        _knockBackEnvironment = GetComponent<KnockBackEnvironmentInteraction>();
        _movement = GetComponent<RunTowardsPlayer>();
        _defensiveStance = GetComponent<DefensiveStance>();

        _activeBaseModel = _baseModel;
    }

    public void SetBaseModel(GameObject model = null, bool specialPose = false)
    {
        if (_activeBaseModel != null)
            _activeBaseModel.SetActive(false);

        _activeBaseModel = model == null ? _baseModel : model;
        _activeBaseModel.SetActive(true);

        _overridesHitPose = specialPose;
        if (specialPose && _getHitRoutine != null && _getHitModel != null)
        {
            _getHitModel.SetActive(false);
            StopCoroutine(_getHitRoutine);
            _getHitRoutine = null;
        }
    }

    public void TakeDmg(float incomingDmg, float knockBack, Vector3 hitDirection)
    {
        if (_defensiveStance != null && _defensiveStance.StanceActive)
        {
            incomingDmg *= _defensiveStance.DmgReduce;
        }

        _currentHP -= incomingDmg;
        _rb.AddForce(hitDirection * knockBack);

        if (_currentHP <= 0)
        {
            DeathSequence();
        } else
        {
            OnDamageTaken(incomingDmg, knockBack);
        }
    }

    protected virtual void OnDamageTaken(float amount, float knockBack)
    {
        _movement.ResetMoveSpeed();

        // If colliding against a wall afterwards shall do damage
        _knockBackEnvironment.CheckInteractionEnable(knockBack);

        // If stance should take damage (if available)
        if (_defensiveStance != null && _defensiveStance.StanceActive)
        {
            _defensiveStance.TakeStanceDamage(knockBack);
            return;
        }
            
        SoundManager.PlaySound(_audio.GetHit);

        if (_overridesHitPose) return;

        if (_getHitRoutine != null)
            StopCoroutine(_getHitRoutine);
        _getHitRoutine = StartCoroutine(ShowHitModel());
    }

    private void ToggleGetHitModel(bool state)
    {
        if (_getHitModel == null) return;

        if (state)
        {
            _getHitModel.SetActive(true);
            _activeBaseModel.SetActive(false);
        } else
        {
            _getHitModel.SetActive(false);
            _activeBaseModel.SetActive(true);
        }
    }

    private IEnumerator ShowHitModel()
    {
        ToggleGetHitModel(true);

        yield return new WaitForSeconds(0.25f);

        ToggleGetHitModel(false);
        _getHitRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCharacter"))
        {
            // If a character is hit, push enemy a bit back
            var character = other.GetComponent<BaseCharacter>();
            TakeDmg(0, 600, -transform.forward);

            character.TakeDmg(_stats.Damage);
        }
    }

    public void DeathSequence()
    {
        SoundManager.PlaySound(_audio.Die);

        Collider collider = transform.GetComponent<Collider>();
        collider.enabled = false;

        // Make sure model dies in damaged pose
        if (_getHitRoutine != null) StopCoroutine(_getHitRoutine);
        if (_defensiveStance) _defensiveStance.StopAllCoroutines();
        //if (_movement) TODO: disable charge effect

        ToggleGetHitModel(true);

        // Dirty fix for now
        // instead have base enemy know about abilities of the enemy and deactivate everything attached to it
        if (_attackType == AttackType.Ranged)
        {
            if (TryGetComponent<ShootAtPlayer>(out ShootAtPlayer rangedAttack))
            {
                rangedAttack.DeactivateAbility();
            }
        }

        Die();
    }

    public void Flee()
    {
        // Remove enemy layer so that it does not become a target while dying
        gameObject.layer = 0;

        _movement.Flee();

        Destroy(gameObject, 6);
    }

    protected virtual void Die()
    {
        OnEnemyDied?.Invoke(this);

        // Remove enemy layer so that it does not become a target while dying
        gameObject.layer = 0;
        Destroy(gameObject, 2);
    }

    private void OnDestroy()
    {
        OnEnemyRemoved?.Invoke(this);
    }
}
