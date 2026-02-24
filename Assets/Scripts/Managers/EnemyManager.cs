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

    public static event System.Action OnWaveSetCompleted;

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

    // ------------
    // Wave
    // ------------

    public void StartWaveSet(WaveSet waveSet)
    {
        StartCoroutine(RunWaveSet(waveSet));
    }

    private IEnumerator RunWaveSet(WaveSet waveSet)
    {
        foreach (EnemyWave wave in waveSet.waves)
        {
            yield return new WaitForSeconds(wave.delayBefore);

            SoundManager.PlaySound(SoundType.WAVE_INCOMING);

            if (wave is BossWave bossWave)
            {
                UiManager.Instance.ShowNewWaveText(true);
                yield return StartCoroutine(SpawnBossWave(bossWave));
            }
            else
            {
                UiManager.Instance.ShowNewWaveText(false);
                yield return StartCoroutine(SpawnWave(wave));
            }

            // Wait until all enemies are dead
            // NOTE: maybe should include a max duration timer
            yield return new WaitUntil(() => !_waveRunning);
            yield return new WaitUntil(() => !_bossWaveRunning);

            yield return new WaitForSeconds(wave.delayAfter);
        }

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
        return SpawnHelper.SpawnEnemyAroundTarget(
            enemyPrefab,
            _playerTransform,
            range.x,
            range.y,
            WorldManager.Instance.GetWorldBounds(),
            LayerMask.GetMask("Obstacle"),
            attempts
        );
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
        RunTowardsPlayer[] enemyMovements = FindObjectsByType<RunTowardsPlayer>(FindObjectsSortMode.None);

        foreach (var e in enemyMovements)
        {
            e.RevertMoveDirection();

            Destroy(e.gameObject, 6);
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
