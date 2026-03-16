using UnityEngine;

public class AttachToPlayer : MonoBehaviour
{
    private void Start()
    {
        Transform player = GameObject.FindWithTag("Player").transform;

        transform.parent = player;
        transform.localPosition = Vector3.zero;
    }
}
