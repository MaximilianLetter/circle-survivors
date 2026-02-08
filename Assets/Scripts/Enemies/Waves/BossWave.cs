using UnityEngine;

[CreateAssetMenu(menuName = "Waves/Boss Wave")]
public class BossWave : EnemyWave
{
    [Header("Boss Info")]
    public GameObject bossPrefab;

    [Tooltip("Delay before boss spawns")]
    public float bossSpawnDelay = 1f;
}
