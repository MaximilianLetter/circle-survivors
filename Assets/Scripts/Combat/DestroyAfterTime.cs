using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    // NOTE: placeholder until using animator and VFX

    [SerializeField] private float _time;

    private void Start()
    {
        Destroy(gameObject, _time);
    }
}
