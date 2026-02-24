using UnityEngine;

public static class SpawnHelper
{
    /// <summary>
    /// Attempts to spawn an enemy at a random position around a target, avoiding obstacles.
    /// </summary>
    /// <param name="enemyPrefab">The enemy prefab to spawn.</param>
    /// <param name="target">The target to spawn around (Usually player)</param>
    /// <param name="minDistance">Minimum distance from the target.</param>
    /// <param name="maxDistance">Maximum distance from the target.</param>
    /// <param name="obstacleLayer">Layer mask for obstacles.</param>
    /// <param name="maxAttempts">Maximum attempts to find a valid spawn point.</param>
    /// <returns>The spawned enemy, or null if failed.</returns>
    public static GameObject SpawnEnemyAroundTarget(
        GameObject enemyPrefab,
        Transform target,
        float minDistance,
        float maxDistance,
        Bounds worldBounds,
        LayerMask obstacleLayer,
        int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float distance = Random.Range(minDistance, maxDistance);
            float angle = Random.Range(-Mathf.PI, Mathf.PI);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            Vector3 spawnPos = target.position + offset;
            spawnPos.y = 0.05f; // Helps with enemies falling through the ground

            if (!worldBounds.Contains(spawnPos))
                continue;

            if (!Physics.CheckSphere(spawnPos, 0.5f, obstacleLayer))
            {
                return Object.Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
        Debug.Log("Failed to find a valid spawn point after " + maxAttempts + " attempts.");
        return null;
    }
}
