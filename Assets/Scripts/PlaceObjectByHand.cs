using System.Collections;
using UnityEngine;

public class PlaceObjectByHand : MonoBehaviour
{
    [SerializeField] private float _dropHeight = 20f;
    [SerializeField] private float _singleDropDuration = 1f;
    [SerializeField] private float _handLeaveDuration = 0.33f;

    [SerializeField] private AnimationCurve _dropCurve;
    [SerializeField] private AnimationCurve _leaveCurve;

    private Vector3 _defaultPosition;

    private void Awake()
    {
        _defaultPosition = transform.position + Vector3.up * _dropHeight;
    }

    public float GetDropHeight()
    {
        return _dropHeight;
    }

    public void DropObject(GameObject obj)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        StartCoroutine(DropObjectCoroutine(obj.transform, _singleDropDuration, true));
    }

    public void LiftObject(GameObject obj)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        StartCoroutine(LiftObjectCoroutine(obj.transform, _singleDropDuration, true));
    }

    public IEnumerator DropObjectCoroutine(Transform objTransform, float dropDuration, bool deactivateAfter = false)
    {
        Vector3 targetPos = objTransform.position;
        targetPos.y = 0f;

        Vector3 startPos = objTransform.position;
        startPos.y = _dropHeight;

        // Hand comes in with object
        objTransform.position = startPos;
        transform.SetPositionAndRotation(startPos, Quaternion.Euler(0, Random.Range(0, 360f), 0));

        float time = 0f;
        while (time < dropDuration)
        {
            time += Time.deltaTime;
            float t = time / dropDuration;

            float curvedT = _dropCurve != null ? _dropCurve.Evaluate(t) : t;

            Vector3 lerpedPos = Vector3.Lerp(startPos, targetPos, curvedT);

            objTransform.position = lerpedPos;
            transform.position = lerpedPos;

            yield return null;
        }
        objTransform.position = targetPos;
        transform.position = targetPos;

        SoundManager.PlaySound(SoundType.HAND_PLACING_CHAR);

        // Hand leaves without object
        startPos = transform.localPosition;
        time = 0f;
        while (time < _handLeaveDuration)
        {
            time += Time.deltaTime;
            float t = time / _handLeaveDuration;

            float curvedT = _leaveCurve != null ? _leaveCurve.Evaluate(t) : t;

            transform.localPosition = Vector3.Lerp(startPos, _defaultPosition, curvedT);

            yield return null;
        }

        if (deactivateAfter) gameObject.SetActive(false);
    }

    public IEnumerator LiftObjectCoroutine(Transform objTransform, float liftDuration, bool deactivateAfter = false)
    {
        gameObject.SetActive(true);

        Vector3 objTargetPos = objTransform.position + Vector3.up * _dropHeight;
        Vector3 objStartPos = objTransform.position;

        // Hand comes in without object
        Vector3 handStartPos = objTargetPos;
        transform.SetPositionAndRotation(handStartPos, Quaternion.Euler(0, Random.Range(0, 360f), 0));

        float time = 0f;
        while (time < _handLeaveDuration)
        {
            time += Time.deltaTime;
            float t = time / _handLeaveDuration;

            float curvedT = _leaveCurve != null ? _leaveCurve.Evaluate(t) : t;

            transform.position = Vector3.Lerp(handStartPos, objStartPos, curvedT);

            yield return null;
        }
        transform.position = objStartPos;

        SoundManager.PlaySound(SoundType.HAND_PLACING_CHAR);

        // Hand leaves with object
        time = 0f;
        while (time < liftDuration)
        {
            time += Time.deltaTime;
            float t = time / liftDuration;

            float curvedT = _dropCurve != null ? _dropCurve.Evaluate(t) : t;

            Vector3 lerpedPos = Vector3.Lerp(objStartPos, objTargetPos, curvedT);

            objTransform.position = lerpedPos;
            transform.position = lerpedPos;

            yield return null;
        }

        objTransform.position = objTargetPos;
        transform.position = objTargetPos;

        if (deactivateAfter) gameObject.SetActive(false);
    }
}
