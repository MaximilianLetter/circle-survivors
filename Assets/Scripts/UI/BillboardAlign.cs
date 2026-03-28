using UnityEngine;

public class BillboardAlign : MonoBehaviour
{
    private void Start()
    {
        Transform camTransform = Camera.main.transform;

        transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
            camTransform.rotation * Vector3.up);
    }
}
