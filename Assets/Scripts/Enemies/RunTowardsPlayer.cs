using UnityEngine;

public class RunTowardsPlayer : MonoBehaviour
{
    [SerializeField] private float _speed;

    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        // todo why enemies stuck in floor?
        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _speed * Time.deltaTime);

        var dir = _playerTransform.position - transform.position;
        dir.Normalize();
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
