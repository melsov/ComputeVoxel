using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace g3
{
    public struct Ray3f
    {
        public Vector3f origin;
        public Vector3f direction;

        public Ray3f(Vector3f origin, Vector3f direction)
        {
            this.origin = origin;
            this.direction = direction.Normalized;
        }

        public Vector3f positionAt(float t)
        {
            return origin + direction * t;
        }

        public static implicit operator Ray3f(Ray r) { return new Ray3f(r.origin, r.direction); }
        public static implicit operator Ray(Ray3f r) { return new Ray(r.origin, r.direction); }
    }
}
