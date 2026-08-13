using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _instance;

    [SerializeField] private Vector2 _farFromPlayer = new Vector2(20f, 22f);
    [SerializeField] private Vector2 _closeToPlayer = new Vector2(5f, 8f);
    [SerializeField] private int _placeByHandCounterTarget = 8;
    [SerializeField] private float _clearBeforeBossTimeout = 15f;

    [SerializeField] private PlaceObjectByHand _placeByHand;

    private Transform _playerTransform;

    private int _aliveEnemiesInWave;
    private readonly HashSet<BaseEnemy> _waveEnemies = new();
    private int _aliveBosses;
    private bool _waveRunning;
    private bool _bossWaveRunning;
    private bool _continuousWave;

    private Vector3? _lastDirection;
    private int _placeByHandCounter;

    public static event System.Action OnWaveSetCompleted;
    public static event System.Action OnWaveStarted;
    public static event System.Action OnWaveFinished;

    public static event System.Action OnEnemyKilled;

    public static EnemyManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("GameManger is NULL");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void OnEnable()
    {
        BaseEnemy.OnEnemyDied += HandleEnemyDeath;
        BaseEnemy.OnEnemyRemoved += HandleEnemyRemoved;
        BossEnemy.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        BaseEnemy.OnEnemyDied -= HandleEnemyDeath;
        BaseEnemy.OnEnemyRemoved -= HandleEnemyRemoved;
        BossEnemy.OnBossDefeated -= HandleBossDefeated;
    }

    // ------------
    // Wave
    // ------------

    public void StartWaveSet(WaveSet waveSet)
    {
        StartCoroutine(RunWaveSet(waveSet));
    }

    public void StartContinuousWave(EnemyWave wave, BossWave boss, bool clearBeforeBoss, float pressureTimer, GameObject pressureEnemy)
    {
        StartCoroutine(SpawnContinuousWave(wave, boss, clearBeforeBoss, pressureTimer, pressureEnemy));
    }

    private IEnumerator RunWaveSet(WaveSet waveSet)
    {
        foreach (EnemyWave wave in waveSet.waves)
        {
            yield return new WaitForSeconds(wave.delayBefore);

            SoundManager.PlaySound(SoundManager.Instance.Library.NewWave);

            yield return new WaitForSeconds(0.25f);
            OnWaveStarted?.Invoke();

            if (wave is BossWave bossWave)
            {
                if (wave.spawnText != string.Empty)
                {
                    WorldTextManager.Instance.ShowDoubleLineWorldText(
                        WorldTextManager.Instance.TextData.newBossWaveText,
                        wave.spawnText
                    );
                }
                else
                    WorldTextManager.Instance.ShowWorldText(WorldTextManager.Instance.TextData.newBossWaveText);

                yield return new WaitForSeconds(2f);
                yield return StartCoroutine(SpawnBossWave(bossWave));
            }
            else
            {
                if (wave.spawnText != string.Empty)
                {
                    WorldTextManager.Instance.ShowDoubleLineWorldText(
                        WorldTextManager.Instance.TextData.newWaveText,
                        wave.spawnText
                    );
                } else
                    WorldTextManager.Instance.ShowWorldText(WorldTextManager.Instance.TextData.newWaveText);

                yield return new WaitForSeconds(2f);
                yield return StartCoroutine(SpawnWave(wave));
            }

            // Wait until all enemies are dead
            // NOTE: maybe should include a max duration timer
            yield return new WaitUntil(() => !_waveRunning);
            yield return new WaitUntil(() => !_bossWaveRunning);

            yield return new WaitForSeconds(1f);
            OnWaveFinished?.Invoke();

            yield return new WaitForSeconds(wave.delayAfter);
        }

        MarkWaveSetAsFinished();
    }

    private IEnumerator SpawnContinuousWave(EnemyWave wave, BossWave bossWave, bool clearBeforeBoss, float pressureTimer, GameObject pressureEnemy)
    {
        yield return new WaitForSeconds(wave.delayBefore);

        SoundManager.PlaySound(SoundManager.Instance.Library.NewWave);
        WorldTextManager.Instance.ShowDoubleLineWorldText(
            WorldTextManager.Instance.TextData.extractionStartText,
            wave.spawnText
        );

        yield return new WaitForSeconds(0.5f);
        OnWaveStarted?.Invoke();

        _waveRunning = true;
        _continuousWave = true;
        ResetWaveEnemies();

        float timer = 0;
        bool pressureOn = false;

        while (_continuousWave)
        {
            GameObject prefab = SelectEnemy(wave.enemies);
            GameObject enemyToSpawn;

            // Every nth enemy, drop them by hand
            _placeByHandCounter++;
            if (_placeByHandCounter >= _placeByHandCounterTarget)
            {
                _placeByHandCounter = 0;
                enemyToSpawn = SpawnEnemy(prefab, _closeToPlayer, 30);
                if (enemyToSpawn != null) _placeByHand.DropObject(enemyToSpawn);
            }
            else
            {
                enemyToSpawn = SpawnEnemy(prefab, _farFromPlayer);
            }

            RegisterWaveEnemy(enemyToSpawn);

            // Pressure timer
            timer += wave.spawnInterval;
            if (timer >= pressureTimer && !pressureOn)
            {
                pressureOn = true;
                SoundManager.PlaySound(SoundManager.Instance.Library.NewWave);
                WorldTextManager.Instance.ShowWorldText(WorldTextManager.Instance.TextData.extractionPressureText);
            }

            if (pressureOn)
            {
                GameObject additionalEnemy = SpawnEnemy(pressureEnemy, _farFromPlayer);
                RegisterWaveEnemy(additionalEnemy);
            }

            yield return new WaitForSeconds(wave.spawnInterval);

            // NOTE: ContinuousWave is set to false from outside
            // Could be listener instead maybe
        }

        // Extraction point reached
        SoundManager.PlaySound(SoundManager.Instance.Library.NewWave); // Placeholder

        // Optional: Kill all remaining enemies
        if (clearBeforeBoss && WaveEnemyCount > 0)
        {
            WorldTextManager.Instance.ShowWorldText(WorldTextManager.Instance.TextData.extractionDoneText);

            float clearTimer = 0f;
            while (WaveEnemyCount > 0 && clearTimer < _clearBeforeBossTimeout)
            {
                clearTimer += Time.deltaTime;
                yield return null;
            }

            if (WaveEnemyCount > 0)
            {
                Debug.LogWarning($"{WaveEnemyCount} wave enemies did not clear before the boss. Removing them so progression can continue.");
                RemoveRemainingWaveEnemies();
            }
        }

        WorldTextManager.Instance.ShowDoubleLineWorldText(
            WorldTextManager.Instance.TextData.newBossWaveText,
            bossWave.spawnText
        );
        yield return new WaitForSeconds(2);

        yield return StartCoroutine(SpawnBossWave(bossWave));

        yield return new WaitUntil(() => !_bossWaveRunning);

        yield return new WaitForSeconds(0.75f);
        OnWaveFinished?.Invoke();

        yield return new WaitForSeconds(wave.delayAfter);

        MarkWaveSetAsFinished();
    }

    private IEnumerator SpawnWave(EnemyWave wave)
    {
        _waveRunning = true;
        _bossWaveRunning = false;
        ResetWaveEnemies();

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject prefab = SelectEnemy(wave.enemies);
            GameObject enemyToSpawn;

            // Every nth enemy, drop them by hand
            _placeByHandCounter++;
            if (_placeByHandCounter >= _placeByHandCounterTarget)
            {
                _placeByHandCounter = 0;
                enemyToSpawn = SpawnEnemy(prefab, _closeToPlayer, 30);
                if (enemyToSpawn != null) _placeByHand.DropObject(enemyToSpawn);
            } else
            {
                enemyToSpawn = SpawnEnemy(prefab, _farFromPlayer);
            }

            RegisterWaveEnemy(enemyToSpawn);

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    private IEnumerator SpawnBossWave(BossWave wave)
    {
        _aliveBosses = 0;
        _bossWaveRunning = true;

        foreach (var boss in wave.bossPrefabs)
        {
            yield return new WaitForSeconds(wave.bossSpawnDelay);
            var spawned = SpawnEnemy(boss, _closeToPlayer, 30, true);
            _placeByHand.DropObject(spawned);
            _aliveBosses++;
        }
    }

    // ------------
    // Enemy
    // ------------

    private GameObject SpawnEnemy(GameObject enemyPrefab, Vector2 range, int attempts = 10, bool forceSpawn = false)
    {
        GameObject enemy = SpawnHelper.SpawnEnemyAroundTarget(
            enemyPrefab,
            _playerTransform,
            range.x,
            range.y,
            WorldManager.Instance.GetWorldBounds(),
            LayerMask.GetMask("Obstacle"),
            out Vector3 newDirection,
            _lastDirection,
            attempts,
            forceSpawn
        );

        if (enemy != null)
            _lastDirection = newDirection.normalized;

        return enemy;
    }

    private void HandleEnemyDeath(BaseEnemy enemy)
    {
        OnEnemyKilled?.Invoke();

        RemoveWaveEnemy(enemy);
    }

    private void HandleEnemyRemoved(BaseEnemy enemy)
    {
        RemoveWaveEnemy(enemy);
    }

    private void RemoveWaveEnemy(BaseEnemy enemy)
    {
        if (!_waveEnemies.Remove(enemy) || !_waveRunning) return;

        if (WaveEnemyCount == 0)
        {
            _waveRunning = false;
        }
    }

    private int WaveEnemyCount
    {
        get
        {
            _waveEnemies.RemoveWhere(enemy => enemy == null);
            _aliveEnemiesInWave = _waveEnemies.Count;
            return _aliveEnemiesInWave;
        }
    }

    private void ResetWaveEnemies()
    {
        _waveEnemies.Clear();
        _aliveEnemiesInWave = 0;
    }

    private void RegisterWaveEnemy(GameObject enemyObject)
    {
        if (enemyObject == null || !enemyObject.TryGetComponent<BaseEnemy>(out var enemy)) return;

        _waveEnemies.Add(enemy);
        _aliveEnemiesInWave = _waveEnemies.Count;
    }

    private void RemoveRemainingWaveEnemies()
    {
        foreach (var enemy in _waveEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        ResetWaveEnemies();
        _waveRunning = false;
    }

    public void StopContinousWave()
    {
        _continuousWave = false;
    }

    private void HandleBossDefeated()
    {
        _aliveBosses--;

        if (_aliveBosses <= 0)
        {
            _bossWaveRunning = false;
            MakeEnemiesWalkAwayAndDie();
        }
    }

    private void MarkWaveSetAsFinished()
    {
        OnWaveSetCompleted?.Invoke();
    }

    private GameObject SelectEnemy(EnemySpawnEntry[] enemies)
    {
        float totalWeight = 0f;

        foreach (var e in enemies)
            totalWeight += e.weight;

        float rng = Random.Range(0, totalWeight);

        foreach (var e in enemies)
        {
            if (rng < e.weight) return e.prefab;

            // Reduce culmulative weight
            rng -= e.weight;
        }

        // Fallback
        return enemies[0].prefab;
    }

    public void MakeEnemiesWalkAwayAndDie()
    {
        // NOTE: enemies should better register themselves at enemyManager when spawned 
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);

        foreach (var e in enemies)
        {
            e.Flee();
        }
    }

    public void KillAllRemainingEnemies()
    {
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);

        foreach (var e in enemies)
        {
            // TODO: stack overflow when creating too many PlayOnce calls
            //e.DeathSequence();
            Destroy(e.gameObject);
        }
    }
}
