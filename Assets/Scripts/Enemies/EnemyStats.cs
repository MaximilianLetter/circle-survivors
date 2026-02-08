using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Base Stats")]
public class EnemyStats : ScriptableObject
{
    public float BaseHp = 18f;
    public float Damage = 6f;

    public float KnockBackThreshold = 500f;
    public float KnockBackDmg = 9f;

    public float MoveSpeed = 2f;
    public float MoveSpeedIncrease = 0.4f;
    public float MoveSpeedIncreaseInterval = 3;

    public SoundType GetHitSound;
    public SoundType DeathSound;

    [Header("Special Abilities")]
    public SoundType SpecialAbilitySound;
}
