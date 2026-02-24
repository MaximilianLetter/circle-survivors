using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Enemy Ranged Stats")]
public class EnemyRangedStats : EnemyStats
{
    [Header("Ranged Attack")]
    public float RangedAttackRange = 4f;
    public float RangedAttackDamage = 6f;
    public float RangedAttackCooldown = 1.5f;
    public GameObject Projectile;
    public SoundType RangedAttackSound;
}
