using System.Collections;
using UnityEngine;

public class ChargeTowardsPlayer : BossAbility
{
    [SerializeField] private float _moveSpeed = 18f;
    [SerializeField] private float _moveDuration = 2f;
    [SerializeField] private float _standStillDuration = 1f;

    [SerializeField] private SFXEntry _makeReadyAudio;
    [SerializeField] private SFXEntry _chargeAudio;
    [SerializeField] private GameObject _chargeModel;

    public override void FireAbility()
    {
        throw new System.NotImplementedException();
    }

    protected override IEnumerator RunAbilityRoutine()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator RunMovementRoutine()
    {
        SoundManager.PlaySound(_makeReadyAudio);
        _baseEnemy.SetBaseModel(_chargeModel, true);

        yield return new WaitForSeconds(_standStillDuration);

        // Get forward once - charge towards that without changing during charge
        Vector3 chargeDir = transform.forward;

        SoundManager.PlaySound(_chargeAudio);
        CameraShake.Instance.TriggerShake(_moveDuration, 0.03f);

        float elapsed = 0f;
        while (elapsed < _moveDuration)
        {
            transform.position += chargeDir * _moveSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;

            yield return null;
        }

        // NOTE: very rarely, the charging enemy can become stuck in a character

        _baseEnemy.SetBaseModel(null);
    }
}
