using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalModifierSystem : MonoBehaviour
{
    private static GlobalModifierSystem _instance;
    public static GlobalModifierSystem Instance => _instance;

    public event Action<StatType> OnModifiersChanged;
    private Dictionary<StatType, List<StatModifier>> _modifiers = new();

    private void Awake()
    {
        _instance = this;
    }

    public void AddModifier(StatModifier modifier)
    {
        if (!_modifiers.ContainsKey(modifier.statType))
            _modifiers[modifier.statType] = new List<StatModifier>();

        _modifiers[modifier.statType].Add(modifier);
        OnModifiersChanged?.Invoke(modifier.statType);

        if (modifier.isTemporary)
        {
            StartCoroutine(RemoveAfterTime(modifier));
        }
    }

    public float ApplyModifiers(StatType type, float baseValue, IStatContext context)
    {
        if (!_modifiers.TryGetValue(type, out var list))
            return baseValue;

        return Helpers.ApplyModifierList(baseValue, list, context);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        _modifiers[modifier.statType].Remove(modifier);
        OnModifiersChanged?.Invoke(modifier.statType);
    }

    private IEnumerator RemoveAfterTime(StatModifier modifier)
    {
        yield return new WaitForSeconds(modifier.duration);

        RemoveModifier(modifier);
    }
}
