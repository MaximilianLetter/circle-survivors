using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    [SerializeField] private Transform[] _collectableSpawnPoints;

    public Transform[] GetSpawnPoints()
    {
        return _collectableSpawnPoints;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (var slot in _collectableSpawnPoints)
        {
            Gizmos.DrawSphere(slot.position, 0.3f);
        }
    }
}
