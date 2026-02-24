using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Base Character Stats")]
public class CharacterStats : ScriptableObject
{
    public float MaxHP = 12f;
    public float InvincibleTimeFrame = 0.25f;

    [Header("Attacks")]
    public float AttackCooldown = 1f;
    public float AttackRange = 12f;
    public float AttackAngle = 90f;
}
