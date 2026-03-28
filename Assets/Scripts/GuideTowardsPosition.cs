using UnityEngine;

public class GuideTowardsTarget : MonoBehaviour
{
    private Transform _target;
    private Transform _objToGuide;
    [SerializeField] private float _indicatorDistance;           // NOTE: Could also use something like screen border 
    [SerializeField] private float _distancePuffer = 2f;

    private void Start()
    {
        _objToGuide = GameObject.FindWithTag("Player").transform;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void Update()
    {
        if (_target == null) return;

        Vector3 direction = _target.position - _objToGuide.position;
        direction.Normalize();

        float distanceToTarget = Vector3.Distance(_target.position, _objToGuide.position);
        float distanceToShow = _indicatorDistance;

        if ((distanceToTarget - _distancePuffer) < distanceToShow)
        {
            distanceToShow = distanceToTarget - _distancePuffer;
        }

        transform.position = _objToGuide.position + direction * distanceToShow;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
