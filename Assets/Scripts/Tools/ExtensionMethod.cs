using UnityEngine;

public static class ExtensionMethod
{
    private const float dotThreshold = 0.5f;
    public static bool IsFacingTarget(this Transform transform, Transform target)
    {
        Vector3 vector2Target = (target.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, vector2Target);
        return dot >= dotThreshold;
    }
}
