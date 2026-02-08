using UnityEngine;

public class RotatingProjectileX : MonoBehaviour
{
    [SerializeField] private float _rotSpeed = 100f;

    private void Update()
    {
        transform.Rotate(_rotSpeed * Time.deltaTime, 0 ,0);
    }
}
