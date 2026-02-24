using System;
using UnityEngine;

[Serializable]
public struct PlantLayer
{
    public GameObject prefab;
    public int count;
    public Vector2 minMaxDistance;
    public bool flatOnGround;
}

public class GrassSpawner : MonoBehaviour
{
    [SerializeField] private PlantLayer[] _plantRows;

    [SerializeField] private Vector2 _xzTilt;
    [SerializeField] private Vector2 _xScaleRange = Vector2.one;
    [SerializeField] private Vector2 _yScaleRange = Vector2.one;

    [SerializeField] private LayerMask _obstacleLayer;

    private void Start()
    {
        SpawnGrassAround();
    }

    private void SpawnGrassAround()
    {
        BoxCollider box = GetComponentInChildren<BoxCollider>();
        if (box == null) return;

        Vector3 localSize = box.size;
        Vector3 localCenter = box.center;
        localCenter.y = 0f;

        //Vector3 center = bounds.center;
        //center.y = 0;

        float halfWidth = localSize.x * 0.5f;
        float halfLength = localSize.z * 0.5f;

        for (int i = 0; i < _plantRows.Length; i++)
        {
            PlantLayer row = _plantRows[i];

            float minOffset = row.minMaxDistance.x;
            float maxOffset = row.minMaxDistance.y;

            float innerHalfWidth = halfWidth + minOffset;
            float innerHalfLength = halfLength + minOffset;

            float outerHalfWidth = halfWidth + maxOffset;
            float outerHalfLength = halfLength + maxOffset;

            int spawned = 0;
            int attempts = 0;
            int maxAttempts = row.count * 10; // safety limit

            while (spawned < row.count && attempts < maxAttempts)
            {
                attempts++;

                // Sample outer rectangle
                float x = UnityEngine.Random.Range(-outerHalfWidth, outerHalfWidth);
                float z = UnityEngine.Random.Range(-outerHalfLength, outerHalfLength);

                // Reject if inside inner rectangle
                if (Mathf.Abs(x) < innerHalfWidth &&
                    Mathf.Abs(z) < innerHalfLength)
                    continue;

                // NOTE: while this looks decent, it could be improved using either rounded edges or 
                // minor noise offset to plants placed

                Vector3 localPoint = new Vector3(x, 0f, z);
                Vector3 worldPoint = box.transform.TransformPoint(localPoint);

                // NOTE: currently, it does not matter if a plant is inside an obstacle, its rather nice that
                // it is close to the building

                //if (Physics.CheckSphere(spawnPosition, 0.1f, _obstacleLayer))
                //    continue;

                SpawnObject(row.prefab, worldPoint, !row.flatOnGround);
                spawned++;
            }
        }
    }

    private void SpawnObject(GameObject obj, Vector3 position, bool randomRot = true)
    {
        Quaternion rotation = randomRot ?
            Quaternion.Euler(UnityEngine.Random.Range(_xzTilt.x, _xzTilt.y), UnityEngine.Random.Range(0, 360f), UnityEngine.Random.Range(_xzTilt.x, _xzTilt.y))
            :
            Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);

        GameObject go = Instantiate(
            obj,
            position,
            rotation,
            transform
        );
        go.transform.localScale = new Vector3(UnityEngine.Random.Range(_xScaleRange.x, _xScaleRange.y), UnityEngine.Random.Range(_yScaleRange.x, _yScaleRange.y), 1);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponentInChildren<BoxCollider>();
        if (box == null) return;

        Gizmos.matrix = box.transform.localToWorldMatrix;

        Vector3 localSize = box.size;
        Vector3 localCenter = box.center;
        localCenter.y = 0f;

        float halfWidth = localSize.x * 0.5f;
        float halfLength = localSize.z * 0.5f;

        foreach (var row in _plantRows)
        {
            float minOffset = row.minMaxDistance.x;
            float maxOffset = row.minMaxDistance.y;

            DrawRectangle(localCenter, halfWidth + minOffset, halfLength + minOffset, Color.yellow);
            DrawRectangle(localCenter, halfWidth + maxOffset, halfLength + maxOffset, Color.green);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawRectangle(Vector3 center, float halfWidth, float halfLength, Color color)
    {
        Gizmos.color = color;

        Vector3 p1 = center + new Vector3(-halfWidth, 0, -halfLength);
        Vector3 p2 = center + new Vector3(-halfWidth, 0, halfLength);
        Vector3 p3 = center + new Vector3(halfWidth, 0, halfLength);
        Vector3 p4 = center + new Vector3(halfWidth, 0, -halfLength);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }

}
