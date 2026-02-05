using UnityEngine;

public class HitQueryGizmos
{
    public static void DrawBoxForward(
        Vector3 origin,
        Vector3 forward,
        BoxHitShape shape,
        Color color
    )
    {
        Vector3 dir = forward.normalized;
        Vector3 center = origin + dir * shape.range * 0.5f;

        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(
            center,
            Quaternion.LookRotation(dir),
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, shape.GetHalfExtents() * 2);
    }
}
