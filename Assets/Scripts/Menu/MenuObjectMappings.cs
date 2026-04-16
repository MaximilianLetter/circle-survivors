using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MenuCharacterMap
{
    public CharacterType type;
    public GameObject passiveModel;
    public GameObject activeModel;
}

[CreateAssetMenu(menuName = "World/Menu Object Mappings")]
public class MenuObjectMappings : ScriptableObject
{
    public MenuCharacterMap[] menuCharacters;
    public GameObject[] rankIndicators;

    private Dictionary<CharacterType, MenuCharacterMap> _lookup;

    private void OnEnable()
    {
        _lookup = new Dictionary<CharacterType, MenuCharacterMap>();

        foreach (var character in menuCharacters)
        {
            if (!_lookup.ContainsKey(character.type))
                _lookup.Add(character.type, character);
            else
                Debug.LogWarning($"Duplicate CharacterType found: {character.type}");
        }
    }

    public GameObject GetPassiveCharacter(CharacterType type)
    {
        return _lookup.TryGetValue(type, out var map)
            ? map.passiveModel
            : null;
    }

    public GameObject GetActiveCharacter(CharacterType type)
    {
        return _lookup.TryGetValue(type, out var map)
            ? map.activeModel
            : null;
    }

    public GameObject GetRankIndicator(int rank)
    {
        if (rank == 0) return null;

        return rankIndicators[rank - 1];
    }
}
