using UnityEngine;

public class CameraShakeOnSpawn : MonoBehaviour
{
    private void Start()
    {
        CameraShake.Instance.TriggerShake(2, 0.05f);
    }
}
