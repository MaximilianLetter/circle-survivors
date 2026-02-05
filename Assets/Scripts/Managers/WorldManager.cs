using UnityEngine;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform _obstacleContainer;
    [SerializeField] private Transform _collectableContainer;

    [Header("Obstacles")]
    [SerializeField] private GameObject[] _smallObstacles;
    [SerializeField] private GameObject[] _mediumObstacles;
    [SerializeField] private GameObject[] _largeObstacles;

    // Could be a custom struct, Vector3 works just as fine although less readable
    [SerializeField] private Vector3 _occuranceWeights;

    [Header("Collectables")]
    [SerializeField] private GameObject[] _characters;
    [SerializeField] private GameObject[] _pickUps;
    [SerializeField] private float _collectableOffset;

    [SerializeField] private int _characterAmount;
    [SerializeField] private int _pickUpAmount;

    [Header("Poisson Disk Sampling")]
    [SerializeField] private float _minDistance = 5f;
    [SerializeField] private Vector2 _mapSize = new Vector2(100f, 100f);
    [SerializeField] private int maxAttempts = 30;

    private void Start()
    {
        GenerateWorld();
    }

    public void GenerateWorld()
    {
        // Generate obstacles as base for the world
        List<Vector2> obstacleSpawnPoints = GeneratePoints();
        obstacleSpawnPoints = CenterPoints(obstacleSpawnPoints, _mapSize);

        foreach (var point in obstacleSpawnPoints)
        {
            Instantiate(
                SelectObstacleByWeight(),
                new Vector3(point.x, 0, point.y),
                Quaternion.Euler(0, Random.Range(0, 360), 0),
                _obstacleContainer
            );
        }

        // Place collectables (characters + pickUps) next to obstacles
        List<Vector2> collectableSpawnPoints = GeneratePointsCollectables(obstacleSpawnPoints, _characterAmount + _pickUpAmount);

        for (int i = 0; i < collectableSpawnPoints.Count; i++)
        {
            Vector2 point = collectableSpawnPoints[i];
            GameObject objToPlace = i >= _pickUpAmount ? _characters[Random.Range(0, _characters.Length)] : _pickUps[Random.Range(0, _pickUps.Length)];

            Instantiate(
                objToPlace,
                new Vector3(point.x, 0, point.y),
                Quaternion.Euler(0, Random.Range(0, 360), 0),
                _collectableContainer
            );
        }
    }

    private GameObject SelectObstacleByWeight()
    {
        var rng = Random.Range(0f, 1f);
        var weights = _occuranceWeights;

        // Use cumulative weights to select
        if (rng < weights.x)
        {
            return SelectRandomFromArray(_smallObstacles);
        }
        else if (rng < weights.y + weights.x)
        {
            return SelectRandomFromArray(_mediumObstacles);
        }
        else
        {
            return SelectRandomFromArray(_largeObstacles);
        }
    }

    private List<Vector2> GeneratePointsCollectables(List<Vector2> obstaclePoints, int amount)
    {
        List<Vector2> collectablePoints = new List<Vector2>();

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomObstaclePoint = obstaclePoints[Random.Range(0, obstaclePoints.Count)];

            // Random direction and distance
            // NOTE: this will result in objects being hidden behind obstacles
            float angle = Random.Range(0f, 2f * Mathf.PI);
            Vector2 offset = new Vector2(Mathf.Cos(angle) * _collectableOffset, Mathf.Sin(angle) * _collectableOffset);

            Vector2 specialObjectPoint = randomObstaclePoint + offset;
            collectablePoints.Add(specialObjectPoint);
        }

        return collectablePoints;
    }

    private GameObject SelectRandomFromArray(GameObject[] array)
    {
        return array[Mathf.FloorToInt(Random.Range(0, array.Length))];
    }

    /// <summary>
    /// Implementation of Poisson Disk Sampling for natural distribution of objects.
    /// </summary>
    /// <returns>Points on 2D plane where objects are placed, need to be transferred in 3D.</returns>
    private List<Vector2> GeneratePoints()
    {
        List<Vector2> points = new List<Vector2>();
        float cellSize = _minDistance / Mathf.Sqrt(2);
        int gridWidth = Mathf.CeilToInt(_mapSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(_mapSize.y / cellSize);
        Vector2[,] grid = new Vector2[gridWidth, gridHeight]; // Initializes 2D array

        // Generate first random point
        Vector2 firstPoint = new Vector2(Random.Range(0, _mapSize.x), Random.Range(0, _mapSize.y));
        points.Add(firstPoint);
        grid[(int)(firstPoint.x / cellSize), (int)(firstPoint.y / cellSize)] = firstPoint;

        // Generate more points
        List<Vector2> activeList = new List<Vector2> { firstPoint };
        while (activeList.Count > 0)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector2 currentPoint = activeList[randomIndex];
            bool found = false;

            // Starting from already set points, try to find other locations to put points
            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = Random.Range(0, 2f * Mathf.PI);
                float distance = Random.Range(_minDistance, 2f * _minDistance);
                Vector2 newPoint = currentPoint + new Vector2(
                    distance * Mathf.Cos(angle),
                    distance * Mathf.Sin(angle)
                );

                // Check if that point is within bounds
                if (newPoint.x < 0 || newPoint.x >= _mapSize.x || newPoint.y < 0 || newPoint.y >= _mapSize.y)
                    continue;

                // Check neighbors in the grid
                int gridX = (int)(newPoint.x / cellSize);
                int gridY = (int)(newPoint.y / cellSize);
                bool tooClose = false;

                for (int gx = Mathf.Max(0, gridX - 1); gx <= Mathf.Min(gridWidth - 1, gridX + 1); gx++)
                {
                    for (int gy = Mathf.Max(0, gridY - 1); gy <= Mathf.Min(gridHeight - 1, gridY + 1); gy++)
                    {
                        if (grid[gx, gy] != Vector2.zero) // Check if cell already contains a point (-> check neighbors)
                        {
                            float dist = Vector2.Distance(newPoint, grid[gx, gy]);
                            if (dist < _minDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                    }
                    if (tooClose) break;
                }

                // The point is fine, not too close to anything, it can be added
                if (!tooClose)
                {
                    points.Add(newPoint);
                    grid[gridX, gridY] = newPoint;
                    activeList.Add(newPoint);
                    found = true;
                    break;
                }
            }

            // If nothing was found starting from this point, it shall not be used for search
            if (!found) activeList.RemoveAt(randomIndex);
        }

        return points;
    }

    private List<Vector2> CenterPoints(List<Vector2> points, Vector2 mapSize)
    {
        Vector2 offset = mapSize * 0.5f;
        List<Vector2> centeredPoints = new List<Vector2>();

        foreach (var p in points)
        {
            centeredPoints.Add(p - offset);
        }

        return centeredPoints;
    }
}
