using System.Collections;
using UnityEngine;

public class SpawnAdds : BossAbility
{
    [SerializeField] private Vector2 _spawnDistance = new Vector2(20, 22);
    [SerializeField] private int _addsPerWave = 4;
    [SerializeField] private float _addSpawnCooldown = 4;

    [SerializeField] private GameObject[] _addPrefabs;
    [SerializeField] private SFXEntry _waveSpawnAudio;

    private Transform _playerTransform;
    private Vector3? _lastAddDirection;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    protected override IEnumerator RunAbilityRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_addSpawnCooldown);

            SpawnAdd();
        }
    }

    public override void FireAbility()
    {
        SpawnWaveOfAdds();
    }

    public override IEnumerator RunMovementRoutine()
    {
        throw new System.NotImplementedException();
    }

    private void SpawnWaveOfAdds()
    {
        SoundManager.PlaySound(_waveSpawnAudio);
        CameraShake.Instance.TriggerShake(2.5f, 0.02f);

        for (int i = 0; i < _addsPerWave; i++)
        {
            SpawnAdd();
        }
    }

    private void SpawnAdd()
    {
        GameObject add = SpawnHelper.SpawnEnemyAroundTarget(
            _addPrefabs[Random.Range(0, _addPrefabs.Length)],
            _playerTransform,
            _spawnDistance.x,
            _spawnDistance.y,
            WorldManager.Instance.GetWorldBounds(),
            LayerMask.GetMask("Obstacle"),
            out Vector3 newDirection,
            _lastAddDirection
        );

        if (add != null)
            _lastAddDirection = newDirection;
    }
}
