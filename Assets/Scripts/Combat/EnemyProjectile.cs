using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private float _timeToLive = 5;
    [SerializeField] private bool _blockable = true;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) Destroy(gameObject);

        if (other.CompareTag("PlayerBlock") && _blockable)
        {
            if (other.TryGetComponent(out BlockProjectiles block))
            {
                if (block.TryToBlock(transform)) return;
            }
        }

        if (other.CompareTag("PlayerCharacter"))
        {
            other.GetComponent<BaseCharacter>().TakeDmg(_dmg);
            Destroy(gameObject);
        }
    }
}
