using g3;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mel.Math
{
    public class EscapeVector
    {

        public static Vector3f Escape(Bounds bounds, Ray3f ray, float nudge = 0f)
        {
            return (EscapeMagnitude(bounds, ray) + nudge) * ray.direction;
        }

        public static Vector3f EscapePositionWithNudge(Bounds bounds, Ray3f ray, float nudge = 0.001f)
        {
            return Escape(bounds, ray, nudge) + ray.origin;
        }

        public static float EscapeMagnitude(Bounds bounds, Ray3f ray)
        {
            return GetCornerMagnitudes(bounds, ray).MinAbs;
        }

        public static bool EnterPosition(Bounds bounds, Ray3f ray, out Vector3f position, float nudge = 0.001f)
        {
            position = Vector3f.Zero;
            float magnitude;
            if (EnterMagnitude(bounds, ray, out magnitude))
            {
                position = ray.origin + ray.direction * (magnitude + nudge);
                return true;
            }
            return false;
        }

        //credit https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection
        public static bool EnterMagnitude(Bounds bounds, Ray3f r, out float enterMagnitude)
        {
            enterMagnitude = 0f;

            Vector3f min = bounds.min;
            Vector3f max = bounds.max;
            float tmin = (min.x - r.origin.x) / r.direction.x;
            float tmax = (max.x - r.origin.x) / r.direction.x;

            if (tmin > tmax) swap(ref tmin, ref tmax);

            float tymin = (min.y - r.origin.y) / r.direction.y;
            float tymax = (max.y - r.origin.y) / r.direction.y;

            if (tymin > tymax) swap(ref tymin, ref tymax);

            if ((tmin > tymax) || (tymin > tmax))
                return false;

            if (tymin > tmin)
                tmin = tymin;

            if (tymax < tmax)
                tmax = tymax;

            float tzmin = (min.z - r.origin.z) / r.direction.z;
            float tzmax = (max.z - r.origin.z) / r.direction.z;

            if (tzmin > tzmax) swap(ref tzmin, ref tzmax);

            if ((tmin > tzmax) || (tzmin > tmax))
                return false;

            if (tzmin > tmin)
                tmin = tzmin;

            if (tzmax < tmax)
                tmax = tzmax;

            enterMagnitude = tmin;
            return true;
        }



        public static Vector3f GetCornerMagnitudes(Bounds bounds, Ray3f ray)
        {
            var corner = (Vector3f)bounds.center + bounds.extents * ray.direction.posToOneNegToNegOne();
            var g = corner - ray.origin;
            return Vector3f.ZeroSafeDivide(g, ray.direction);
        }

        static void swap(ref float a, ref float b)
        {
            float temp = a;
            a = b;
            b = temp;
        }


    }
}
