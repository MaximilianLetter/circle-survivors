using UnityEngine;

public class DeathFall : MonoBehaviour
{
    [SerializeField] private float _fallDistance = 0.5f;
    [SerializeField] private float _fallSpeed = 2f;
    [SerializeField] private float _rotateSpeed = 90f;

    private bool _dying = false;
    private Vector3 _startLocalPos;
    private Quaternion _startLocalRot;
    private void Update()
    {
        if (!_dying) return;

        // Placeholder until Animator is used
        Vector3 targetPos = _startLocalPos + Vector3.down * _fallDistance;
        Quaternion targetRot = _startLocalRot * Quaternion.Euler(90f, 0f, 0f);

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, _fallSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot, _rotateSpeed * Time.deltaTime);
    }

    public void PlayDeathAnimation()
    {
        _startLocalPos = transform.localPosition;
        _startLocalRot = transform.localRotation;

        _dying = true;
        Destroy(gameObject, 2f);
    }
}
