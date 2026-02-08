using UnityEngine;

public class RotatingProjectile : MonoBehaviour
{
    [SerializeField] private float _rotSpeed = 100f;

    private void Update()
    {
        transform.Rotate(Vector3.forward, _rotSpeed * Time.deltaTime);
    }
}
