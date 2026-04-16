using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance => _instance;
    private static WorldManager _instance;

    [SerializeField] private GameObject _worldGround;

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
    [SerializeField] private GameObject _cornerPrefab;

    [Header("Collectables")]
    [SerializeField] private ObjectMappings _objMappings;
    [SerializeField] private GameObject[] _pickUps;
    [SerializeField] private float _collectableOffset;

    [Header("Poisson Disk Sampling")]
    private float _minDistance = 4f;
    private float _mapEdgePuffer = 14f;
    private int _maxAttempts = 30;

    private Vector2 _mapSpawnArea;
    private Bounds _mapSpawnBounds;
    private GameObject _boundaryContainer;

    //private List<Vector2> _obstacleSpawnPoints;
    private List<BaseObstacle> _placedObstacles;

    // Could be a custom struct, Vector3 works just as fine although less readable
    private Vector3 _occuranceWeights;

    private int _characterAmount;
    private int _characterModifierAmount;
    private int _partyIncreaseModifierAmount;
    private int _pickUpHealAmount;

    private Vector2 _mapSize;
    private Vector2 _startingPoint;
    private List<Vector2> _keepFreeZones;
    private BiomeConfig _biome;

    private GameObject _activeWeatherEffect;
    private GameObject _extractionPoint;
    private GameObject _extractionPointGuidance;

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
        _biome = config.biomeConfig;
        _mapSize = config.mapSize;
        _minDistance = config.minDistance;
        _startingPoint = config.playerStartPos;
        _keepFreeZones.Add(_startingPoint);

        _characterAmount = config.characterAmount;
        _characterModifierAmount = config.characterModifierAmount;
        _pickUpHealAmount = config.pickUpHealthAmount;
        _partyIncreaseModifierAmount = config.partyIncreaseModifierAmount;

        _occuranceWeights = config.obstacleWeights;

        _mapSpawnArea = _mapSize - new Vector2(_mapEdgePuffer, _mapEdgePuffer);
        _mapSpawnBounds = new Bounds(Vector3.zero, new Vector3(_mapSize.x, 10f, _mapSize.y));

        // Add extraction point if required
        if (config.type == LevelType.Extraction)
        {
            Vector3 spawnPos = new Vector3(config.extractionPointPosition.x, 0, config.extractionPointPosition.y);
            _extractionPoint = Instantiate(config.extractionPoint, spawnPos, Quaternion.identity);
            _extractionPointGuidance = Instantiate(config.extractionGuidance);

            _keepFreeZones.Add(config.extractionPointPosition);

            // Set references to each other
            var target = _extractionPointGuidance.GetComponent<GuideTowardsTarget>();
            target.SetTarget(_extractionPoint.transform);

            var endzone = _extractionPoint.GetComponent<ExtractionPoint>();
            endzone.SetGuidanceRef(target);
        }

        // Generate obstacles and collectables (based on the obstacles)
        GenerateObstacles();
        GenerateCollectables();

        ScaleGroundObject();

        BuildWorldBounds();

        // Add weather effects if present
        if (config.weatherEffectPrefab != null)
        {
            _activeWeatherEffect = Instantiate(config.weatherEffectPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    private List<Vector2> GenerateObstacles()
    {
        List<Vector2> obstacleSpawnPoints = GeneratePoints();
        obstacleSpawnPoints = CenterPoints(obstacleSpawnPoints, _mapSpawnArea);

        foreach (var point in obstacleSpawnPoints)
        {
            GameObject newObstacle = Instantiate(
                SelectObstacleByWeight(),
                new Vector3(point.x, 0, point.y),
                Quaternion.Euler(0, Random.Range(0, 360), 0),
                _obstacleContainer
            );

            _placedObstacles.Add(newObstacle.GetComponent<BaseObstacle>());
        }

        return obstacleSpawnPoints;
    }

    private void GenerateCollectables()
    {
        List<Transform> availableSlots = new List<Transform>();

        foreach (BaseObstacle obstacle in _placedObstacles)
        {
            availableSlots.AddRange(obstacle.GetSpawnPoints());
        }

        // Shuffle slots
        for (int i = 0; i < availableSlots.Count; i++)
        {
            int randomIndex = Random.Range(i, availableSlots.Count);
            (availableSlots[i], availableSlots[randomIndex]) =
                (availableSlots[randomIndex], availableSlots[i]);
        }

        int totalCollectablesAmount = _characterAmount + _characterModifierAmount + _pickUpHealAmount + _partyIncreaseModifierAmount;

        int slotIndex = 0;
        int spawnedCount = 0;

        while (slotIndex < availableSlots.Count)
        {
            int totalRemaining =
                _characterAmount +
                _characterModifierAmount +
                _pickUpHealAmount +
                _partyIncreaseModifierAmount;

            if (totalRemaining == 0)
                break;

            Transform slot = availableSlots[slotIndex];
            slotIndex++;

            GameObject objToPlace = GetNextPickup();

            Vector3 pos = slot.position;
            Quaternion rot = slot.rotation;

            if (!CheckIfPositionIsFree(pos))
                continue;

            Instantiate(objToPlace, pos, rot, _collectableContainer);
            spawnedCount++;
        }

        if (spawnedCount < totalCollectablesAmount)
        {
            Debug.LogError("Not enough spawn slots for all collectables!" + spawnedCount + "/" + totalCollectablesAmount);
        }
    }

    public GameObject GetNextPickup()
    {
        int totalRemaining = _characterAmount + _characterModifierAmount + _pickUpHealAmount + _partyIncreaseModifierAmount;
        if (totalRemaining == 0)
        {
            return null;
        }

        int rng = Random.Range(0, totalRemaining);
        GameObject prefab;

        if (rng < _characterAmount)
        {
            prefab = _objMappings.GetRandomCollectableCharacter();
            _characterAmount--;
        }
        else if (rng < _characterAmount + _characterModifierAmount)
        {
            prefab = _objMappings.GetCollectablePickup(CollectableType.StatModifier);
            _characterModifierAmount--;
        }
        else if (rng < _characterAmount + _characterModifierAmount + _pickUpHealAmount)
        {
            prefab = _objMappings.GetCollectablePickup(CollectableType.HealthPickUp);
            _pickUpHealAmount--;
        }
        else
        {
            prefab = _objMappings.GetCollectablePickup(CollectableType.PartyIncrease);
            _partyIncreaseModifierAmount--;
        }

        return prefab;
    }

    public void ClearWorld()
    {
        _keepFreeZones = new List<Vector2>();

        // Obstacles
        foreach (Transform child in _obstacleContainer)
            Destroy(child.gameObject);

        _placedObstacles = new List<BaseObstacle>();

        // Collectables
        foreach (Transform child in _collectableContainer)
            Destroy(child.gameObject);

        // Boundaries
        if (_boundaryContainer != null)
            Destroy(_boundaryContainer);

        // Specials
        if (_extractionPoint != null)
        {
            // TODO can be made cleaner
            Destroy(_extractionPoint);
            //Destroy(_extractionPointGuidance);
            // ^ guidance point should destroy itself
        }

        if (_activeWeatherEffect != null)
            Destroy(_activeWeatherEffect);
    }

    private void ScaleGroundObject()
    {
        // NOTE: base model is 200x200 big
        float factorX = _mapSize.x / 200f;
        float factorZ = _mapSize.y / 200f;
        _worldGround.transform.localScale = new Vector3(factorX, 1, factorZ);

        // NOTE: base texture tiling is 10x10
        Material worldGroundMaterial = _worldGround.GetComponent<Renderer>().material;
        worldGroundMaterial.mainTextureScale = new Vector2(10 * factorX, 10 * factorZ);
    }

    public void PlaceCollectableCharacter(Vector3 position, Quaternion rotation, CharacterType charType)
    {
        if (!CheckIfPositionIsFree(position)) return;

        GameObject collCharacter = _objMappings.GetCollectableCharacter(charType);
        if (collCharacter == null) return;

        Instantiate(
            collCharacter,
            position,
            rotation,
            _collectableContainer
        );
    }

    private bool CheckIfPositionIsFree(Vector3 pos)
    {
        float halfX = _mapSize.x * 0.5f;
        float halfZ = _mapSize.y * 0.5f;

        // Check world bounds first
        if (pos.x < -halfX || pos.x > halfX ||
            pos.z < -halfZ || pos.z > halfZ)
        {
            return false;
        }

        // Should always pass as placements are handcrafted next to obstacles, could also be deleted
        return !Physics.CheckSphere(pos, 0.25f, _obstacleLayer);
    }

    private GameObject SelectObstacleByWeight()
    {
        Vector3 weights = _occuranceWeights;
        float sumOfWeights = _occuranceWeights.x + _occuranceWeights.y + _occuranceWeights.z;
        float rng = Random.Range(0f, sumOfWeights);

        // Use cumulative weights to select
        if (rng < weights.x)
        {
            return SelectRandomFromArray(_biome.smallObstacles);
        }
        else if (rng < weights.y + weights.x)
        {
            return SelectRandomFromArray(_biome.mediumObstacles);
        }
        else
        {
            return SelectRandomFromArray(_biome.largeObstacles);
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

        Vector2 center = new Vector2(_mapSpawnArea.x / 2f, _mapSpawnArea.y / 2f);

        List<Vector2> keepFreeZonesWorld = new List<Vector2>();

        // This holds starthing position and potentially extraction point
        foreach (Vector2 zone in _keepFreeZones)
        {
            keepFreeZonesWorld.Add(center + zone);
        }

        float exclusionRadius = 10f;

        // Generate first random pointVector2 firstPoint;
        Vector2 firstPoint;
        do
        {
            firstPoint = new Vector2(Random.Range(0, _mapSpawnArea.x), Random.Range(0, _mapSpawnArea.y));
        } while (IsInsideAnyExclusion(firstPoint, keepFreeZonesWorld, exclusionRadius));

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

                // Check if the new point has space to the starting point
                if (IsInsideAnyExclusion(newPoint, keepFreeZonesWorld, exclusionRadius))
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

    private bool IsInsideAnyExclusion(Vector2 point, List<Vector2> zones, float radius)
    {
        foreach (Vector2 zone in zones)
        {
            if (Vector2.Distance(point, zone) < radius)
                return true;
        }
        return false;
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
        float halfX = _mapSize.x * 0.5f;
        float halfZ = _mapSize.y * 0.5f;

        float offset = 10;

        Vector3[] corners = new Vector3[]
        {
            new Vector3(-halfX + offset, 0.5f, -halfZ + offset), // bottom-left
            new Vector3(-halfX + offset, 0.5f,  halfZ - offset), // top-left (original)
            new Vector3( halfX - offset, 0.5f,  halfZ - offset), // top-right
            new Vector3( halfX - offset, 0.5f, -halfZ + offset), // bottom-right
        };

        // Spawn corner prefabs
        for (int i = 0; i < corners.Length; i++)
        {
            Quaternion rotation = Quaternion.identity;

            switch (i)
            {
                case 1: // top-left (original)
                    rotation = Quaternion.Euler(0f, 0f, 0f);
                    break;
                case 2: // top-right
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    break;
                case 3: // bottom-right
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case 0: // bottom-left
                    rotation = Quaternion.Euler(0f, 270f, 0f);
                    break;
            }

            Instantiate(_cornerPrefab, corners[i], rotation, lines.transform);
        }
    }

    public bool IsInsideBounds(Vector3 pos, Vector3 halfExtents)
    {
        float halfX = _mapSize.x * 0.5f;
        float halfZ = _mapSize.y * 0.5f;

        return pos.x - halfExtents.x >= -halfX &&
               pos.x + halfExtents.x <= halfX &&
               pos.z - halfExtents.z >= -halfZ &&
               pos.z + halfExtents.z <= halfZ;
    }
}
