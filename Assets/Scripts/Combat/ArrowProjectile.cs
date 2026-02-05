using UnityEngine;

public class ArrowProjectile : BaseProjectile
{
    protected override void OnEnemyHit(BaseEnemy enemy)
    {
        Destroy(gameObject);
    }
}
