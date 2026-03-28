using UnityEngine;

public class SpawnFlyOverBirds : MonoBehaviour
{
    [SerializeField] private GameObject _birdPrefab;
    [SerializeField] private float _birdHeight = 20f;
    [SerializeField] private Vector2 _spawnRateRange = new Vector2(5, 25);

    private void Start()
    {
        float initialSpawnDealy = 5f + Random.Range(_spawnRateRange.x, _spawnRateRange.y);

        Invoke(nameof(SpawnBird), initialSpawnDealy);
    }

    private void SpawnBird()
    {
        GameObject bird = SpawnHelper.SpawnEnemyAroundTarget(
            _birdPrefab, transform, 30, 35,
            new Bounds(Vector3.zero, Vector3.positiveInfinity),
            LayerMask.GetMask(), out _);

        Vector3 pos = bird.transform.localPosition;
        pos.y = _birdHeight;
        Quaternion rot = Quaternion.LookRotation(transform.position - bird.transform.position);

        bird.transform.SetLocalPositionAndRotation(pos, rot);

        float nextBird = Random.Range(_spawnRateRange.x, _spawnRateRange.y);
        Invoke(nameof(SpawnBird), nextBird);
    }
}
