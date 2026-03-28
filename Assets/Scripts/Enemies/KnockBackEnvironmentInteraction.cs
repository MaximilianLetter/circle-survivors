using UnityEngine;

[RequireComponent(typeof(BaseEnemy))]
[RequireComponent(typeof(Collider))]
public class KnockBackEnvironmentInteraction : MonoBehaviour
{
    [SerializeField] private float _knockBackTurnOffDelay = 1f;

    private BaseEnemy _baseEnemy;
    private bool _knockBackByPlayer;

    private void Start()
    {
        _baseEnemy = GetComponent<BaseEnemy>();
    }

    public void CheckInteractionEnable(float knockBack)
    {
        if (knockBack >= _baseEnemy.Stats.KnockBackThreshold)
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

            float dmg = _baseEnemy.Stats.KnockBackDmg * (forceSqr / _baseEnemy.Stats.KnockBackThreshold);
            _baseEnemy.TakeDmg(dmg, 100, -transform.forward); // Knockback could alternatively be 0, test of bounce effect
            _knockBackByPlayer = false;

            SoundManager.PlaySound(_baseEnemy.Audio.WallImpact);
            CameraShake.Instance.TriggerShake(0.5f, 0.025f);
        }
    }
}
