using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private WaveSet _waveSet;

    [SerializeField] private float _minDistance = 15f;
    [SerializeField] private float _maxDistance = 20f;


    private Transform _playerTransform;

    private int _aliveEnemiesInWave;
    private bool _waveRunning;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;

        StartCoroutine(RunWaveSet());
    }

    private void OnEnable()
    {
        BaseEnemy.OnEnemyDied += HandleEnemyDeath;
        BossEnemy.OnBossDefeated += KillAllRemainingEnemies;
    }

    private void OnDisable()
    {
        BaseEnemy.OnEnemyDied -= HandleEnemyDeath;
        BossEnemy.OnBossDefeated -= KillAllRemainingEnemies;
    }

    // ------------
    // Wave
    // ------------

    private IEnumerator RunWaveSet()
    {
        foreach (EnemyWave wave in _waveSet.waves)
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

            yield return new WaitForSeconds(wave.delayAfter);
        }
    }

    private IEnumerator SpawnWave(EnemyWave wave)
    {
        _waveRunning = true;
        _aliveEnemiesInWave = 0;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject prefab = SelectEnemy(wave.enemies);
            GameObject enemyToSpawn = SpawnEnemy(prefab);

            _aliveEnemiesInWave++;

            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    private IEnumerator SpawnBossWave(BossWave wave)
    {
        // TODO: boss sometimes falls though ground
        yield return new WaitForSeconds(wave.bossSpawnDelay);
        SpawnEnemy(wave.bossPrefab);
    }

    // ------------
    // Enemy
    // ------------

    private GameObject SpawnEnemy(GameObject enemyPrefab)
    {
        float distance = Random.Range(_minDistance, _maxDistance);
        float angle = Random.Range(-Mathf.PI, Mathf.PI);

        Vector3 enemyPos = _playerTransform.position + new Vector3( Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
        return Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
    }

    private void HandleEnemyDeath(BaseEnemy enemy)
    {
        if (!_waveRunning) return;

        _aliveEnemiesInWave--;

        if (_aliveEnemiesInWave <= 0)
        {
            _waveRunning = false;
        }
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

    private void KillAllRemainingEnemies()
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
