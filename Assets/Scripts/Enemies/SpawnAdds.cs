using System.Collections;
using UnityEngine;

public class SpawnAdds : MonoBehaviour
{
    [SerializeField] private Vector2 _spawnDistance;
    [SerializeField] private float _spawnInterval;
    [SerializeField] private int _addsPerWave;
    [SerializeField] private GameObject _addPrefab;

    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
        StartCoroutine(SpawnAddsLoop());
    }

    private void OnDestroy()
    {
        // Note this should rather be hooked up with health/death system of
        // base enemy or boss enemy that inherits from base enemy
        StopAllCoroutines();
    }

    private IEnumerator SpawnAddsLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);

            // Spawn one wolf every interval, spawn a lot of wolves when triggered by hp threshold
            SpawnAdd();
        }
    }

    public void TriggerSpecialAbility()
    {
        for (int i = 0; i < _addsPerWave; i++)
        {
            SpawnAdd();
        }
    }

    private void SpawnAdd()
    {
        SpawnHelper.SpawnEnemyAroundTarget(
            _addPrefab,
            _playerTransform,
            _spawnDistance.x,
            _spawnDistance.y,
            WorldManager.Instance.GetWorldBounds(),
            LayerMask.GetMask("Obstacle")
        );
    }
}
