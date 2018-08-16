using UnityEngine;

namespace Mel.Math
{
    public static class VecExt
    {
        //
        //Vec3
        //
        public static Vector3 RandomVector3(float absRange = 1f) { return new Vector3(Random.Range(-absRange, absRange), Random.Range(-absRange, absRange), Random.Range(-absRange, absRange)); }

        public static Vector3 Inverse(this Vector3 v) { return new Vector3(1f / v.x, 1f / v.y, 1f / v.z); }

        public static Vector3 DivideBy(this Vector3 a, Vector3 b) { return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z); }

        public static Vector3 Times(this Vector3 a, Vector3 b) { return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z); }

        public static float MinAbs(this Vector3 a) { return Mathf.Min(Mathf.Abs(a.x), Mathf.Min(Mathf.Abs(a.y), Mathf.Abs(a.z))); }
        //
        //Vec4
        // 
        public static Vector3 XYZ(this Vector4 v) { return new Vector3(v.x, v.y, v.z); }
    }
}
