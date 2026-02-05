using System;
using UnityEngine;

[Serializable]
public struct BoxHitShape
{
    public float range;
    public float width;
    public float height;

    public readonly Vector3 GetHalfExtents()
        => new(width * 0.5f, height * 0.5f, range * 0.5f);
}

public static class HitQuery
{
    public static Collider[] BoxForward(
        Vector3 origin,
        Vector3 forward,
        BoxHitShape shape,
        LayerMask targetMask
    )
    {
        Vector3 dir = forward.normalized;
        Vector3 center = origin + dir * shape.range * 0.5f;

        return Physics.OverlapBox(
            center,
            shape.GetHalfExtents(),
            Quaternion.LookRotation(dir),
            targetMask
        );
    }
}
