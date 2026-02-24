using UnityEngine;

public class BillboardAlign : MonoBehaviour
{
    private Transform _cameraTransform;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;

        transform.LookAt(transform.position + _cameraTransform.rotation * Vector3.forward,
            _cameraTransform.rotation * Vector3.up);
    }
}
