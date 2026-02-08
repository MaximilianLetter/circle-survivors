using UnityEngine;

[RequireComponent(typeof(BaseEnemy))]
[RequireComponent(typeof(Collider))]
public class KnockBackEnvironmentInteraction : MonoBehaviour
{
    [SerializeField] private EnemyStats _stats;
    [SerializeField] private float _knockBackTurnOffDelay = 2f;

    [SerializeField] private SoundType _impactSound;

    private BaseEnemy _baseEnemy;
    private bool _knockBackByPlayer;

    private void Start()
    {
        _baseEnemy = GetComponent<BaseEnemy>();
    }

    public void CheckInteractionEnable(float knockBack)
    {
        if (knockBack >= _stats.KnockBackThreshold)
        {
            _knockBackByPlayer = true;
            Invoke(nameof(TurnOffKnockBackFlag), _knockBackTurnOffDelay);
        }
    }

    private void TurnOffKnockBackFlag()
    {
        _knockBackByPlayer = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_knockBackByPlayer) return;

        if (collision.collider.CompareTag("Obstacle"))
        {
            var forceSqr = collision.impulse.sqrMagnitude;

            float dmg = _stats.KnockBackDmg * (forceSqr / _stats.KnockBackThreshold);
            _baseEnemy.TakeDmg(dmg, 0);
            _knockBackByPlayer = false;

            SoundManager.PlaySound(_impactSound);
        }
    }
}
