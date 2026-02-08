using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Ranged Enemy Stats")]
public class RangedEnemyStats : EnemyStats
{
    [Header("Ranged Attack")]
    public float RangedAttackRange = 4f;
    public float RangedAttackDamage = 6f;
    public float RangedAttackCooldown = 1.5f;
    public GameObject Projectile;
    public SoundType RangedAttackSound;
}
