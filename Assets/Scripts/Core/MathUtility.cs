using UnityEngine;
namespace Guisha.Core
{
    public static class MathUtility
    {
        public static Vector3 Planar(Vector3 value) => new Vector3(value.x, 0, value.z);
        public static bool InRange(Vector3 a, Vector3 b, float radius) => (a - b).sqrMagnitude <= radius * radius;
        public static bool InCone(Vector3 origin, Vector3 forward, Vector3 point, float range, float angle)
            => InRange(origin, point, range) && Vector3.Angle(forward, point - origin) <= angle * 0.5f;
        public static float ExpFactor(float sharpness, float deltaTime) => 1 - Mathf.Exp(-Mathf.Max(0, sharpness) * deltaTime);
        public static Vector3 Curve(Vector3 start, Vector3 control, Vector3 end, float t)
        { t = Mathf.Clamp01(t); return (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end; }
    }
}
