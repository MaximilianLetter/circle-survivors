using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CharacterMap
{
    public CharacterType type;
    public GameObject collectablePrefab;
    public GameObject playablePrefab;
}

[CreateAssetMenu(menuName = "World/Object Mappings")]
public class ObjectMappings : ScriptableObject
{
    public CharacterMap[] characters;

    public GameObject healthPickup;
    public GameObject[] modifierPickups;
    public GameObject partyIncreasePickup;

    private Dictionary<CharacterType, CharacterMap> _lookup;

    private void OnEnable()
    {
        _lookup = new Dictionary<CharacterType, CharacterMap>();

        foreach (var character in characters)
        {
            if (!_lookup.ContainsKey(character.type))
                _lookup.Add(character.type, character);
            else
                Debug.LogWarning($"Duplicate CharacterType found: {character.type}");
        }
    }

    public GameObject GetCollectableCharacter(CharacterType type)
    {
        return _lookup.TryGetValue(type, out var map)
            ? map.collectablePrefab
            : null;
    }

    public GameObject GetPlayableCharacter(CharacterType type)
    {
        return _lookup.TryGetValue(type, out var map)
            ? map.playablePrefab
            : null;
    }

    public GameObject GetRandomCollectableCharacter()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning("No characters configured in ObjectMappings.");
            return null;
        }

        int index = UnityEngine.Random.Range(0, characters.Length);
        return characters[index].collectablePrefab;
    }

    public GameObject GetCollectablePickup(CollectableType type)
    {
        switch (type)
        {
            case CollectableType.HealthPickUp:
                return healthPickup;

            case CollectableType.StatModifier:
                int rng = UnityEngine.Random.Range(0, modifierPickups.Length);
                return modifierPickups[rng];

            case CollectableType.PartyIncrease:
                return partyIncreasePickup;

            default:
                return healthPickup;
        }
    }
}
