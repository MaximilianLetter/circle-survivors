using UnityEngine;

public class BirdFlight : MonoBehaviour
{
    [SerializeField] private float _speed = 25f;
    [SerializeField] private float _timeToLive = 4f;
    [SerializeField] private float _turnSpeed = 60f;

    [SerializeField] private SFXEntry _soundFx;

    private void Start()
    {
        Destroy(gameObject, _timeToLive);
        _turnSpeed = Random.Range(0, _turnSpeed);

        SoundManager.PlaySound(_soundFx, 0.5f);
    }

    private void Update()
    {
        transform.Rotate(0f, _turnSpeed * Time.deltaTime, 0f);
        transform.position += _speed * Time.deltaTime * transform.forward;
    }
}
