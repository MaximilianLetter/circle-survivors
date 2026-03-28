using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _instance;

    [SerializeField] private Vector2 _farFromPlayer = new Vector2(20f, 22f);
    [SerializeField] private Vector2 _closeToPlayer = new Vector2(5f, 8f);

    [SerializeField] private PlaceObjectByHand _placeByHand;

    private Transform _playerTransform;

    private int _aliveEnemiesInWave;
    private bool _waveRunning;
    private bool _bossWaveRunning;
    private bool _continuousWave;

    private Vector3? _lastDirection;

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
        BossEnemy.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        BaseEnemy.OnEnemyDied -= HandleEnemyDeath;
        BossEnemy.OnBossDefeated -= HandleBossDefeated;
    }

    public bool GetWaveRunningState()
    {
        return (_waveRunning || _bossWaveRunning);
    }

    // ------------
    // Wave
    // ------------

    public void StartWaveSet(WaveSet waveSet)
    {
        StartCoroutine(RunWaveSet(waveSet));
    }

    public void StartContinuousWave(EnemyWave wave, BossWave boss)
    {
        StartCoroutine(SpawnContinuousWave(wave, boss));
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
                WorldTextManager.Instance.ShowDoubleLineWorldText(
                    WorldTextManager.Instance.TextData.newBossWaveText,
                    LevelManager.Instance.ConfigInUse.bossText,
                    null
                );
                yield return new WaitForSeconds(2f);
                yield return StartCoroutine(SpawnBossWave(bossWave));
            }
            else
            {
                WorldTextManager.Instance.ShowWorldText(WorldTextManager.Instance.TextData.newWaveText, null);
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(SpawnWave(wave));
            }

            // Wait until all enemies are dead
            // NOTE: maybe should include a max duration timer
            yield return new WaitUntil(() => !_waveRunning);
            yield return new WaitUntil(() => !_bossWaveRunning);

            yield return new WaitForSeconds(0.75f);
            OnWaveFinished?.Invoke();

            yield return new WaitForSeconds(wave.delayAfter);
        }

        MarkWaveSetAsFinished();
    }

    private IEnumerator SpawnContinuousWave(EnemyWave wave, BossWave bossWave)
    {
        yield return new WaitForSeconds(wave.delayBefore);

        SoundManager.PlaySound(SoundManager.Instance.Library.NewWave);
        WorldTextManager.Instance.ShowWorldText(WorldTextManager.Instance.TextData.extractionStartText, null);

        yield return new WaitForSeconds(0.25f);
        OnWaveStarted?.Invoke();

        _waveRunning = true;
        _continuousWave = true;
        _aliveEnemiesInWave = 0;

        while (_continuousWave)
        {
            GameObject prefab = SelectEnemy(wave.enemies);
            GameObject enemyToSpawn = SpawnEnemy(prefab, _farFromPlayer);

            if (enemyToSpawn != null)
                _aliveEnemiesInWave++;

            yield return new WaitForSeconds(wave.spawnInterval);

            // ContinuousWave is set to false from outside
            // Should be listener instead maybe
        }

        // Variant A: Kill all remaining enemies
        //UiManager.Instance.ShowExtractionText(false);
        //SoundManager.PlaySound(SoundManager.Instance.Library.CollectPickUp); // Placeholder
        //yield return new WaitUntil(() => _aliveEnemiesInWave == 0);

        // Variant B: Spawn boss and wait for him to be defeated

        //UiManager.Instance.ShowNewWaveText(true);
        WorldTextManager.Instance.ShowDoubleLineWorldText(
            WorldTextManager.Instance.TextData.newBossWaveText,
            LevelManager.Instance.ConfigInUse.bossText,
            null
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
        _aliveEnemiesInWave = 0;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject prefab = SelectEnemy(wave.enemies);
            GameObject enemyToSpawn = SpawnEnemy(prefab, _farFromPlayer);

            if (enemyToSpawn != null)
                _aliveEnemiesInWave++;

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    private IEnumerator SpawnBossWave(BossWave wave)
    {
        _bossWaveRunning = true;

        yield return new WaitForSeconds(wave.bossSpawnDelay);
        var boss = SpawnEnemy(wave.bossPrefab, _closeToPlayer, 30);
        _placeByHand.DropObject(boss);
    }

    // ------------
    // Enemy
    // ------------

    private GameObject SpawnEnemy(GameObject enemyPrefab, Vector2 range, int attempts = 10)
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
            attempts
        );

        if (enemy != null)
            _lastDirection = newDirection.normalized;

        return enemy;
    }

    private void HandleEnemyDeath(BaseEnemy enemy)
    {
        OnEnemyKilled?.Invoke();

        if (!_waveRunning) return;

        _aliveEnemiesInWave--;

        if (_aliveEnemiesInWave <= 0)
        {
            _waveRunning = false;
        }
    }

    public void StopContinousWave()
    {
        _continuousWave = false;
    }

    private void HandleBossDefeated()
    {
        _bossWaveRunning = false;
        MakeEnemiesWalkAwayAndDie();
        //MarkWaveSetAsFinished();
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
