using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modifiers exclusive to the Character/Enemy using it.
/// To be used for individual Buffs/Debuffs/LevelUps.
/// </summary>
public class LocalModifierSystem : MonoBehaviour
{
    public event Action<StatType> OnModifiersChanged;

    private Dictionary<StatType, List<StatModifier>> _modifiers
        = new();

    public void AddModifier(StatModifier modifier)
    {
        if (!_modifiers.ContainsKey(modifier.statType))
            _modifiers[modifier.statType] = new List<StatModifier>();

        _modifiers[modifier.statType].Add(modifier);
        OnModifiersChanged?.Invoke(modifier.statType);
    }

    // TODO: remove Modifier logic

    public float ApplyModifiers(
        StatType type,
        float baseValue,
        IStatContext context)
    {
        if (!_modifiers.TryGetValue(type, out var list))
            return baseValue;

        return Helpers.ApplyModifierList(baseValue, list, context);
    }
}
