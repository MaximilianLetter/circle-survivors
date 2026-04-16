using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Enemy Ranged Stats")]
public class EnemyRangedStats : EnemyStats
{
    [Header("Ranged Attack")]
    public float RangedAttackRange = 4f;
    public float RangedAttackDamage = 6f;
    public float RangedAttackCooldown = 1.5f;
    public float RangedAttackPrecision = 10f; // TODO
    public GameObject Projectile;
}
