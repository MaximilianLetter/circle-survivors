using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _startDelay = 5f;
    [SerializeField] private float _repeatDelay = 2f;
    [SerializeField] private float _minDistance = 5f;
    [SerializeField] private float _maxDistance = 20f;

    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;

        InvokeRepeating(nameof(SpawnEnemy), _startDelay, _repeatDelay);
    }

    private void SpawnEnemy()
    {
        float distance = Random.Range(_minDistance, _maxDistance);
        float angle = Random.Range(-Mathf.PI, Mathf.PI);

        Vector3 enemyPos = _playerTransform.position + new Vector3( Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
        Instantiate(_enemyPrefab, enemyPos, Quaternion.identity);
    }
}
