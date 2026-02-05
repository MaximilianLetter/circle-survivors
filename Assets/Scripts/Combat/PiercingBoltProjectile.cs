using UnityEngine;

public class PiercingBoltProjectile : BaseProjectile
{
    [Header("Piercing")]
    [SerializeField] private int _maxPierces = 5;

    private int _piercedCount = 0;

    protected override void OnEnemyHit(BaseEnemy enemy)
    {
        _piercedCount++;

        if (_piercedCount >= _maxPierces)
        {
            Destroy(gameObject);
        }
    }
}
