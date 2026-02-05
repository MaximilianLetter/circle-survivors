using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CharacterPrefabEntry
{
    public CharacterType type;
    public GameObject prefab;
}

public class CircleOfCharacters : MonoBehaviour
{
    [Header("CharacterPrefabs")]
    [SerializeField] private CharacterPrefabEntry[] _prefabs;
    [SerializeField] private CharacterType _startAsType;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private float _radius = 0.75f;
    [SerializeField] private float _radiusIncrease = 0.25f;

    private int _amountOfCharacters = 0;
    private Dictionary<CharacterType, GameObject> _characterPrefabs;
    private List<GameObject> _characters = new List<GameObject>();
    private SphereCollider _playerCollider;

    private void Awake()
    {
        // NOTE: Workaround to have a serialized Dictionary, evenutally use Scriptable Objects
        _characterPrefabs = new Dictionary<CharacterType, GameObject>();

        foreach (var prefab in _prefabs)
        {
            _characterPrefabs.Add(prefab.type, prefab.prefab);
        }
    }

    private void Start()
    {
        _playerCollider = _playerMovement.GetComponent<SphereCollider>();
        AddCharacter(_startAsType);
    }

    public void AddCharacter(CharacterType type)
    {
        _amountOfCharacters++;

        GameObject newOne = Instantiate(_characterPrefabs[type]);
        newOne.transform.parent = transform;
        _characters.Add(newOne);

        RearrangeCircle();

        if (_amountOfCharacters > 1) GameManager.Instance.AdjustCameraSize(true);
    }

    public void RemoveCharacter(GameObject dyingCharacter)
    {
        _amountOfCharacters--;

        _characters.Remove(dyingCharacter);
        dyingCharacter.transform.parent = null;

        GameManager.Instance.AdjustCameraSize(false);

        RearrangeCircle();

        if (_amountOfCharacters == 0)
        {
            GameManager.Instance.EndGame();
            return;
        }
    }

    private void RearrangeCircle()
    {
        float rotAngle = 360f / _amountOfCharacters;
        float radius = _radius + (_amountOfCharacters - 1) * _radiusIncrease;
        if (_amountOfCharacters == 1) radius = 0; // No offset if only a single character

        for (int i = 0; i < _amountOfCharacters; i++)
        {
            float segment = 2 * Mathf.PI * i / _amountOfCharacters;

            _characters[i].transform.SetLocalPositionAndRotation(
                new Vector3(Mathf.Sin(segment) * radius, 0, Mathf.Cos(segment) * radius),
                Quaternion.Euler(0, rotAngle * i % 360f, 0)
            );

            // Not used by all abilities, but helps with knowing where your base rotation is
            _characters[i].GetComponent<CharacterAbility>().UpdateBaseRotation();
        }

        // Adjust collider for terrain collision & pickups
        _playerCollider.radius = (_amountOfCharacters == 1) ? 0.5f : radius;
        _playerMovement.AdjustTurnSpeed(_amountOfCharacters);
    }

    public void RestoreHpForAllCharacters()
    {
        // NOTE: GetComponent call not great during runtime
        // list could maybe be List<BaseCharacter> instead
        foreach (var c in _characters)
        {
            c.GetComponent<BaseCharacter>().RestoreHp();
        }
    }
}
