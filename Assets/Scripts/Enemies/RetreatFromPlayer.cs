using System.Collections;
using UnityEngine;

public class RetreatFromPlayer : BossAbility
{
    [SerializeField] private float _moveDistance = 4f;
    [SerializeField] private float _moveDuration = 2f;
    [SerializeField] private SFXEntry _retreatAudio;

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
        float elapsed = 0f;
        Vector3 retreatDir = -transform.forward;

        SoundManager.PlaySound(_retreatAudio);

        while (elapsed < _moveDuration)
        {
            transform.position += retreatDir * _moveDistance * Time.deltaTime;
            elapsed += Time.deltaTime;

            yield return null;
        }
    }
}
