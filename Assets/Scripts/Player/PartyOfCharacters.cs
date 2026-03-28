using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyOfCharacters : MonoBehaviour, IStatContext
{
    // NOTE: in order to apply modificators to the party, need to integrate context interface
    [HideInInspector] public Faction Faction => Faction.Player;
    [HideInInspector] public AttackType AttackType => AttackType.None;
    [HideInInspector] public CharacterType CharacterType => CharacterType.None;

    [SerializeField] private CharacterType _startAsType;
    [SerializeField] private ObjectMappings _mappings;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PartyStats _stats;
    [SerializeField] private float _radius = 0.75f;
    [SerializeField] private float _radiusIncrease = 0.25f;

    [SerializeField] private PlayerUI _playerUI;
    [SerializeField] private Transform _directionIndicator;

    private int _maxPartySize;
    private int AmountOfCharacters => _characters.Count;

    private List<GameObject> _characters = new List<GameObject>();
    private CapsuleCollider _playerCollider;
    private SmoothCameraSizeAdjustment _cameraAdjustment;

    public static event Action OnCharacterAdded;

    private void Start()
    {
        _playerCollider = _playerMovement.GetComponent<CapsuleCollider>();
        _cameraAdjustment = Camera.main.GetComponent<SmoothCameraSizeAdjustment>();

        // NOTE: Party of Characters always exists - subscription can be done in Start instead of OnEnable/OnDisable
        if (GlobalModifierSystem.Instance != null)
            GlobalModifierSystem.Instance.OnModifiersChanged += OnGlobalModifiersChanged;
        RecalculateMaxPartySize();

        // Use serialized type only for testing, normally use None type to set it here in code afterwards
        if (_startAsType == CharacterType.None)
        {
            if (GameManager.Instance.Mode == GameMode.Tutorial)
            {
                _startAsType = CharacterType.Knight;
            }
            else
            {
                _startAsType = (CharacterType)UnityEngine.Random.Range(1, 4); // NOTE: not super clean, 0 is None
            }
        }

        AddCharacter(_startAsType);
        _playerMovement.SetMoveSpeed(false); // Start in idle phase
    }

    private void OnEnable()
    {
        EnemyManager.OnWaveStarted += HandleWaveStart;
        EnemyManager.OnWaveFinished += HandleWaveEnd;
    }

    private void OnDisable()
    {
        EnemyManager.OnWaveStarted -= HandleWaveStart;
        EnemyManager.OnWaveFinished -= HandleWaveEnd;
    }

    private void OnDestroy()
    {
        if (GlobalModifierSystem.Instance != null)
                GlobalModifierSystem.Instance.OnModifiersChanged -= OnGlobalModifiersChanged;
    }

    public float ResolveStat(StatType type, int baseValue)
    {
        float value = baseValue;

        value = GlobalModifierSystem.Instance
            .ApplyModifiers(type, value, this);

        // Does not have a local modifier system, use global for modifying party

        return value;
    }

    private void OnGlobalModifiersChanged(StatType type)
    {
        if (type == StatType.PartySize)
        {
            RecalculateMaxPartySize();
            _playerUI.SetCharacterAmountText(AmountOfCharacters, _maxPartySize);
        }
    }

    private void RecalculateMaxPartySize()
    {
        _maxPartySize = (int)ResolveStat(StatType.PartySize, _stats.MaxPartySize);
    }

    public bool CanAddCharacter()
    {
        return AmountOfCharacters < _maxPartySize;
    }

    public void AddCharacter(CharacterType type)
    {
        GameObject newOne = Instantiate(_mappings.GetPlayableCharacter(type), transform);
        _characters.Add(newOne);

        RearrangeCircle();

        _cameraAdjustment.AdjustCameraSize(true);
        _playerUI.SetCharacterAmountText(AmountOfCharacters, _maxPartySize);

        OnCharacterAdded?.Invoke();
    }

    public void RemoveCharacter(GameObject dyingCharacter)
    {
        _characters.Remove(dyingCharacter);
        dyingCharacter.transform.parent = null;

        RearrangeCircle();

        _cameraAdjustment.AdjustCameraSize(false);
        _playerUI.SetCharacterAmountText(AmountOfCharacters, _maxPartySize);

        if (AmountOfCharacters == 0)
        {
            GameManager.Instance.LoseGame();
            return;
        }
    }

    public void ExchangeCharacter(CharacterType charType, BaseCharacter exchangingCharacter, Vector3 pos, Quaternion rot)
    {
        float hpPercentage = exchangingCharacter.GetHpPercentage();

        // Remove old one
        int index = _characters.IndexOf(exchangingCharacter.gameObject);
        _characters.Remove(exchangingCharacter.gameObject);
        Destroy(exchangingCharacter.gameObject);

        // Add new one
        GameObject newOne = Instantiate(_mappings.GetPlayableCharacter(charType), transform);
        _characters.Insert(index, newOne);

        newOne.GetComponent<BaseCharacter>().SetHpPercentage(hpPercentage);

        RearrangeCircle();

        // Leave behind exchanged character for pick up
        WorldManager.Instance.PlaceCollectableCharacter(pos, rot, exchangingCharacter.CharacterType);
    }

    // TODO change different formation between fights

    // TODO Animate formation change (maybe)

    // TODO clean up a bit (make this singleton and more)

    public void RearrangeCircle()
    {
        int amount = AmountOfCharacters;

        float rotAngle = 360f / amount;
        float radiusIncrease = (amount - 1) * _radiusIncrease;
        float radius = _radius + radiusIncrease;
        if (amount == 1) radius = 0; // No offset if only a single character

        for (int i = 0; i < amount; i++)
        {
            float segment = 2 * Mathf.PI * i / amount;

            _characters[i].transform.SetLocalPositionAndRotation(
                new Vector3(Mathf.Sin(segment) * radius, 0, Mathf.Cos(segment) * radius),
                Quaternion.Euler(0, rotAngle * i % 360f, 0)
            );

            // Not used by all abilities, but helps with knowing where your base rotation is
            _characters[i].GetComponent<CharacterAbility>().UpdateBaseRotation();
        }

        // Adjust collider for terrain collision & pickups
        _playerCollider.radius = (amount == 1) ? 0.5f : 1 + radiusIncrease;
        _playerMovement.AdjustTurnSpeed(amount);

        _directionIndicator.localPosition = Vector3.forward * (1 + radius);
    }

    private void HandleWaveStart()
    {
        SetPartyFightState(true);
    }

    private void HandleWaveEnd()
    {
        SetPartyFightState(false);
    }

    public void SetPartyFightState(bool fightState)
    {
        StartCoroutine(SetPartyFightStateRoutine(fightState));
    }

    private IEnumerator SetPartyFightStateRoutine(bool fightState)
    {
        _playerMovement.SetMoveSpeed(fightState);

        float delayBetweenCharacters = 0.1f;
        foreach (var cGO in _characters)
        {
            // Toggle all visual states
            BaseCharacter character = cGO.GetComponent<BaseCharacter>();

            SoundManager.PlaySound(character.Audio.StanceChange);

            if (fightState)
                character.ApplyVisualState(ModelVariant.Fight);
            else
                character.ResetCharacterState(); // Includes special resource reset

            yield return new WaitForSeconds(delayBetweenCharacters + UnityEngine.Random.Range(0f, 0.03f));
        }
    }

    public void ResetParty()
    {
        foreach(var c in _characters)
        {
            Destroy(c);
        }
        _characters.Clear();

        // Start with one random character
        CharacterType startType = (CharacterType)UnityEngine.Random.Range(1, 4);
        AddCharacter(startType);
    }

    public List<GameObject> GetAllCharacters()
    {
        return _characters;
    }
}
