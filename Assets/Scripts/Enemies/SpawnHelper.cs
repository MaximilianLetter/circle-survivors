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
        out Vector3 newDirection,
        Vector3? lastDirection = null,
        int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float distance = Random.Range(minDistance, maxDistance);
            
            // Only try to go onto opposite direction on initial placement
            if (lastDirection.HasValue && i == 0)
            {
                Vector3 opposite = -lastDirection.Value;
                float variation = 60f;
                newDirection = Quaternion.Euler(0, Random.Range(-variation, variation), 0) * opposite;
                newDirection = newDirection.normalized * distance;
            } else
            {
                float angle = Random.Range(-Mathf.PI, Mathf.PI);
                newDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            }

            Vector3 spawnPos = target.position + newDirection;
            spawnPos.y = 0.05f; // Helps with enemies falling through the ground

            if (!worldBounds.Contains(spawnPos))
                continue;

            if (!Physics.CheckSphere(spawnPos, 0.5f, obstacleLayer))
            {
                return Object.Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }

        Debug.Log("Failed to find a valid spawn point after " + maxAttempts + " attempts.");
        newDirection = Vector3.zero; // Fallback
        return null;
    }
}
