using System;
using UnityEngine;

public enum ModifierOperation
{
    Add,
    Multiply
}

[Serializable]
public class StatModifier
{
    public StatType statType;
    public ModifierOperation operation;
    public float value;

    [Header("Targeting")] // None = any
    public Faction targetFaction = Faction.None;
    public AttackType targetAttackType = AttackType.None;
    public CharacterType targetCharacterType = CharacterType.None;

    public bool isTemporary;
    public float duration;

    public StatModifier(StatModifierSO data)
    {
        statType = data.StatType;
        operation = data.Operation;
        value = data.Value;

        targetFaction = data.TargetFaction;
        targetAttackType = data.TargetAttackType;
        targetCharacterType = data.TargetCharacterType;

        isTemporary = data.IsTemporary;
        duration = data.Duration;
    }
}