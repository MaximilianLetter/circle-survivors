using System.Collections;
using UnityEngine;

public class PullOverMap : MonoBehaviour
{
    [SerializeField] private Vector3 _targetPos;
    [SerializeField] private Vector3 _startPos;
    [SerializeField] private Transform _handWithMap;
    [SerializeField] private SFXEntry _pageSound;

    public void TriggerMapPull(Vector3 playerPos, float duration)
    {
        gameObject.SetActive(true);

        _handWithMap.parent.position = playerPos;
        SoundManager.PlaySound(_pageSound);

        _handWithMap.gameObject.SetActive(true);
        StartCoroutine(MapPullRoutine(duration));
    }

    private IEnumerator MapPullRoutine(float duration)
    {
        _handWithMap.localPosition = _startPos;

        float t = 0f;
        while (t < duration)
        {
            float progress = t / duration;

            Vector3 pos = Vector3.Lerp(_targetPos, _startPos, Mathf.SmoothStep(0, 1, progress));
            _handWithMap.localPosition = pos;

            t += Time.deltaTime;
            yield return null;
        }

        _handWithMap.localPosition = _targetPos;
        _handWithMap.gameObject.SetActive(false);
    }
}
