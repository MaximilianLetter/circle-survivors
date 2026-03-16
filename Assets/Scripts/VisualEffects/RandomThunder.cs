using System.Collections;
using UnityEngine;

public class RandomThunder : MonoBehaviour
{
    [SerializeField] private Vector2 _timeRange;
    [SerializeField] private SFXEntry _soundFx;

    private void Start()
    {
        StartCoroutine(ThunderRoutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator ThunderRoutine()
    {
        while (true)
        {
            float waitToNextThunder = Random.Range(_timeRange.x, _timeRange.y);

            yield return new WaitForSeconds(waitToNextThunder);

            TriggerThunderEffect();
        }
    }

    private void TriggerThunderEffect()
    {
        SoundManager.PlaySound(_soundFx);
        CameraShake.Instance.TriggerShake(5f, 0.02f, true);
    }
}
