using UnityEngine;

namespace g3
{
    public static class SafeDivide
    {
        public static float ZeroSafeDivide(float numerator, float divisor)
        {
            if(Mathf.Abs(divisor) < Mathf.Epsilon) { return divisor * numerator < 0f ? float.MinValue : float.MaxValue; }
            return numerator / divisor;
        }
    }
}
