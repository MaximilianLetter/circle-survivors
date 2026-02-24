using UnityEngine;

[CreateAssetMenu(menuName = "Modifiers/Stat Modifier")]
public class StatModifierSO : ScriptableObject
{
    [Header("Effect")]
    public StatType StatType;
    public ModifierOperation Operation;
    public float Value;

    [Header("Target (None is all)")]
    public Faction TargetFaction;
    public AttackType TargetAttackType;
    public CharacterType TargetCharacterType;

    [Header("Temporary Buff")]
    public bool IsTemporary;
    public float Duration;

    public StatModifier CreateRuntimeInstance()
    {
        return new StatModifier(this);
    }
}
