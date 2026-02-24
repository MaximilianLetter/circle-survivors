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

    public GameObject GetCollectable(CharacterType type)
    {
        return _lookup.TryGetValue(type, out var map)
            ? map.collectablePrefab
            : null;
    }

    public GameObject GetPlayable(CharacterType type)
    {
        return _lookup.TryGetValue(type, out var map)
            ? map.playablePrefab
            : null;
    }

    public GameObject GetRandomCollectable()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning("No characters configured in ObjectMappings.");
            return null;
        }

        int index = UnityEngine.Random.Range(0, characters.Length);
        return characters[index].collectablePrefab;
    }
}
