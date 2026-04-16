using UnityEngine;

[CreateAssetMenu(menuName = "Waves/Enemy Wave")]
public class EnemyWave : ScriptableObject
{
    public float delayBefore = 2f;
    public float delayAfter = 1f;

    public int enemyCount = 10;
    public float spawnInterval = 0.5f;

    [TextArea(3, 5)]
    public string spawnText = string.Empty;

    public EnemySpawnEntry[] enemies;
}