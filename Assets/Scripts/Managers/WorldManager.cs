using System.Collections.Generic;
using UnityEngine;


public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance => _instance;
    private static WorldManager _instance;

    [Header("Containers")]
    [SerializeField] private Transform _obstacleContainer;
    [SerializeField] private Transform _collectableContainer;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private LayerMask _collectableLayer;

    [Header("World Boundaries")]
    [SerializeField] private float _wallThickness = 1f;
    [SerializeField] private float _wallHeight = 5f;
    [SerializeField] private bool _showEndOfWorldIndicators = true;
    [SerializeField] private Material _lineMaterial;

    [Header("Obstacles")]
    [SerializeField] private GameObject[] _smallObstacles;
    [SerializeField] private GameObject[] _mediumObstacles;
    [SerializeField] private GameObject[] _largeObstacles;

    [Header("Collectables")]
    [SerializeField] private ObjectMappings _objMappings;
    [SerializeField] private GameObject[] _pickUps;
    [SerializeField] private float _collectableOffset;

    [Header("Poisson Disk Sampling")]
    private float _minDistance = 4f;
    private float _mapEdgePuffer = 10f;
    private int _maxAttempts = 30;

    private Vector2 _mapSpawnArea;
    private Bounds _mapSpawnBounds;
    private GameObject _boundaryContainer;

    private List<Vector2> _obstacleSpawnPoints;
    private List<Vector2> _collectableSpawnPoints;

    // Could be a custom struct, Vector3 works just as fine although less readable
    private Vector3 _occuranceWeights;
    private int _characterAmount;
    private int _pickUpAmount;
    private Vector2 _mapSize;

    private void Awake()
    {
        _instance = this;
    }

    public Bounds GetWorldBounds()
    {
        return _mapSpawnBounds;
    }

    public LayerMask GetObstacleLayer()
    {
        return _obstacleLayer;
    }

    public void GenerateWorld(LevelConfig config)
    {
        // Set all map values
        _mapSize = config.mapSize;
        _minDistance = config.minDistance;
        _characterAmount = config.characterAmount;
        _pickUpAmount = config.pickUpAmount;
        _occuranceWeights = config.obstacleWeights;

        _mapSpawnArea = _mapSize - new Vector2(_mapEdgePuffer, _mapEdgePuffer);
        _mapSpawnBounds = new Bounds(Vector3.zero, new Vector3(_mapSize.x, 10f, _mapSize.y));

        ClearWorld();

        // Generate obstacles and collectables (based on the obstacles)
        _obstacleSpawnPoints = GenerateObstacles();
        _collectableSpawnPoints = GenerateCollectables();

        BuildWorldBounds();
    }

    private List<Vector2> GenerateObstacles()
    {
        List<Vector2> obstacleSpawnPoints = GeneratePoints();
        obstacleSpawnPoints = CenterPoints(obstacleSpawnPoints, _mapSpawnArea);

        foreach (var point in obstacleSpawnPoints)
        {
            Instantiate(
                SelectObstacleByWeight(),
                new Vector3(point.x, 0, point.y),
                Quaternion.Euler(0, Random.Range(0, 360), 0),
                _obstacleContainer
            );
        }

        return obstacleSpawnPoints;
    }

    private List<Vector2> GenerateCollectables()
    {
        List<Vector2> spawnPoints = GeneratePointsCollectables(_obstacleSpawnPoints, _characterAmount + _pickUpAmount);

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            const int maxAttempts = 5;
            bool spawned = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 basePoint = spawnPoints[i];
                Vector2 randomOffset = Random.insideUnitCircle * (1f + (attempt * 0.5f));

                Vector3 pos = new Vector3(
                    basePoint.x + randomOffset.x,
                    0f,
                    basePoint.y + randomOffset.y
                );

                if (!CheckIfPositionIsFree(pos, true))
                    continue;


                GameObject objToPlace = i >= _pickUpAmount ? _objMappings.GetRandomCollectable() : _pickUps[Random.Range(0, _pickUps.Length)];
                Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);

                Instantiate(objToPlace, pos, rot, _collectableContainer);

                spawned = true;
                break;
            }

            if (!spawned) Debug.Log($"Failed to spawn collectable at index {i}");
        }

        return spawnPoints;
    }

    public void ClearWorld()
    {
        foreach (Transform child in _obstacleContainer)
            Destroy(child.gameObject);

        foreach (Transform child in _collectableContainer)
            Destroy(child.gameObject);

        if (_boundaryContainer != null)
            Destroy(_boundaryContainer);
    }

    public void PlaceCollectableCharacter(Vector3 position, Quaternion rotation, CharacterType charType)
    {
        if (!CheckIfPositionIsFree(position)) return;

        GameObject collCharacter = _objMappings.GetCollectable(charType);
        if (collCharacter == null) return;

        Instantiate(
            collCharacter,
            position,
            rotation,
            _collectableContainer
        );
    }

    private bool CheckIfPositionIsFree(Vector3 pos, bool checkCollectables = false)
    {
        if (checkCollectables && Physics.CheckSphere(pos, 1.5f, _collectableLayer))
            return false;

        return !Physics.CheckSphere(pos, 0.5f, _obstacleLayer);
    }

    private GameObject SelectObstacleByWeight()
    {
        Vector3 weights = _occuranceWeights;
        float sumOfWeights = _occuranceWeights.x + _occuranceWeights.y + _occuranceWeights.z;
        float rng = Random.Range(0f, sumOfWeights);

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
        int gridWidth = Mathf.CeilToInt(_mapSpawnArea.x / cellSize);
        int gridHeight = Mathf.CeilToInt(_mapSpawnArea.y / cellSize);
        Vector2[,] grid = new Vector2[gridWidth, gridHeight]; // Initializes 2D array

        // Define the center and exclusion radius, center needs to stay empty
        Vector2 center = new Vector2(_mapSpawnArea.x / 2f, _mapSpawnArea.y / 2f);
        float exclusionRadius = 6f;

        // Generate first random pointVector2 firstPoint;
        Vector2 firstPoint;
        do
        {
            firstPoint = new Vector2(Random.Range(0, _mapSpawnArea.x), Random.Range(0, _mapSpawnArea.y));
        } while (Vector2.Distance(firstPoint, center) < exclusionRadius);

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
            for (int i = 0; i < _maxAttempts; i++)
            {
                float angle = Random.Range(0, 2f * Mathf.PI);
                float distance = Random.Range(_minDistance, 2f * _minDistance);
                Vector2 newPoint = currentPoint + new Vector2(
                    distance * Mathf.Cos(angle),
                    distance * Mathf.Sin(angle)
                );

                // Check if that point is within bounds
                if (newPoint.x < 0 || newPoint.x >= _mapSpawnArea.x || newPoint.y < 0 || newPoint.y >= _mapSpawnArea.y)
                    continue;

                // Check if the new point is within the exclusion zone
                if (Vector2.Distance(newPoint, center) < exclusionRadius)
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

    private void BuildWorldBounds()
    {
        _boundaryContainer = new GameObject("WorldBounds");

        Vector3 center = Vector3.zero;
        float halfX = _mapSize.x * 0.5f;
        float halfZ = _mapSize.y * 0.5f;

        // Left
        CreateWall(
            new Vector3(-halfX - _wallThickness * 0.5f, 0f, 0f),
            new Vector3(_wallThickness, _wallHeight, _mapSize.y)
        );

        // Right
        CreateWall(
            new Vector3(halfX + _wallThickness * 0.5f, 0f, 0f),
            new Vector3(_wallThickness, _wallHeight, _mapSize.y)
        );

        // Top
        CreateWall(
            new Vector3(0f, 0f, halfZ + _wallThickness * 0.5f),
            new Vector3(_mapSize.x, _wallHeight, _wallThickness)
        );

        // Bottom
        CreateWall(
            new Vector3(0f, 0f, -halfZ - _wallThickness * 0.5f),
            new Vector3(_mapSize.x, _wallHeight, _wallThickness)
        );

        if (_showEndOfWorldIndicators) CreateBoundaryLines();
    }

    private void CreateWall(Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.parent = _boundaryContainer.transform;
        wall.transform.position = position;

        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.size = size;
    }

    private void CreateBoundaryLines()
    {
        GameObject lines = new GameObject("BoundaryLines");
        lines.transform.parent = _boundaryContainer.transform;

        LineRenderer lr = lines.AddComponent<LineRenderer>();
        lr.material = _lineMaterial;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lr.positionCount = 5;
        lr.loop = true;
        lr.widthMultiplier = 0.2f;

        float halfX = _mapSize.x * 0.5f;
        float halfZ = _mapSize.y * 0.5f;

        lr.SetPositions(new Vector3[]
        {
            new Vector3(-halfX, 0.1f, -halfZ),
            new Vector3(-halfX, 0.1f,  halfZ),
            new Vector3( halfX, 0.1f,  halfZ),
            new Vector3( halfX, 0.1f, -halfZ),
            new Vector3(-halfX, 0.1f, -halfZ),
        });
    }
}
