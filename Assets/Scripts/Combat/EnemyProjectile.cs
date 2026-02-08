using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private float _timeToLive = 5;
    [SerializeField] private SoundType _sound;

    private float _dmg;

    protected virtual void Start()
    {
        Destroy(gameObject, _timeToLive);
    }

    protected virtual void Update()
    {
        transform.position += _speed * Time.deltaTime * transform.forward;
    }

    public void SetValues(float dmg)
    {
        _dmg = dmg;
    }

    public float GetDmgStat()
    {
        return _dmg;
    }

    // TODO: getting damage is currently done by getting base enemy script -> handle that in characters, also destroy this gameobject on hit

    // NOTE: Dmg application is done in Characters themselves

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) Destroy(gameObject);
    }
}
