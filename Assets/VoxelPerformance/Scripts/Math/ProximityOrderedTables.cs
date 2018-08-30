using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Mel.Math
{
    public struct ProximityOrderCoords
    {
        IntVector3[] coords;

        public ProximityOrderCoords(int cubeSize)
        {
            coords = ProximityOrderedTables.ProximityOrderedIntVectors(cubeSize);
        }

        public ProximityOrderCoords(IntVector3 cube)
        {
            coords = ProximityOrderedTables.ProximityOrderedIntVectors(cube);
        }

        public ProximityOrderCoords(IntVector3 cube, IntVector3 boundsSize)
        {
            coords = ProximityOrderedTables.ProximityOrderedBoundsOrigins(cube, boundsSize);
        }

        public IntVector3 this[int i] { get { return coords[i]; } }

        public IEnumerable<IntVector3> Iterator {
            get {
                foreach(var c in coords)
                {
                    yield return c;
                }
            }
        }

        public int Length {
            get { return coords.Length; }
        }

    }

    public class ProximityOrderedTables : MonoBehaviour
    {

        [MenuItem("MEL/Table to Console")]
        static void TableToConsole()
        {
            StringBuilder sb = new StringBuilder();
            int sz = 4;
            var prox = ProximityOrderedIntVectors(sz);
            Debug.Log("size: " + sz + " cubed: " + Mathf.Pow(sz, 3) + " mOne cubed: " + Mathf.Pow(sz - 1, 3));
            Debug.Log(prox.Length);
            foreach (var iv in prox)
            {
                sb.Append(FormatIntVector3Decl(iv));
            }
            Debug.Log(sb.ToString());
        }

        public static IntVector3[] ProximityOrderedBoundsOrigins(IntVector3 cubeSize, IntVector3 boundsSize)
        {
            var scaledSize = cubeSize / boundsSize;
            var iter = ProximityOrderedIntVectors(scaledSize);
            var result = new IntVector3[iter.Length];
            for(int i=0; i<result.Length; ++i)
            {
                result[i] = iter[i] * boundsSize;
            }
            return result;
        }

        public static IntVector3[] ProximityOrderedIntVectors(IntVector3 cubeSize)
        {
            var bounds = IntBounds3.FromCenterHalfSize(IntVector3.zero, cubeSize / 2);
            var iter = ProximityOrderedIntVectors(cubeSize.MaxComponent);
            var result = new List<IntVector3>(cubeSize.Area);
            foreach(var v in iter)
            {
                if (bounds.Contains(v))
                {
                    result.Add(v);
                }
            }
            return result.ToArray();
        }

        public static IntVector3[] ProximityOrderedIntVectors(int cubeSize)
        {
            IntBounds3 b = new IntBounds3
            {
                start = IntVector3.zero,
                size = new IntVector3(1)
            };

            IntBounds3 p = new IntBounds3
            {
                start = b.start,
                size = IntVector3.zero
            };

            var ivs = new List<IntVector3>(cubeSize * cubeSize * cubeSize);

            while(b.size.x <= cubeSize )
            {
                foreach(var iv in b.IteratorYXZ)
                {
                    if(p.Contains(iv)) { continue; }
                    ivs.Add(iv);
                }
                p = b;
                b = b.ExpandedBordersAdditive(new IntVector3(1));
            }

            if (cubeSize % 2 == 0)
            {
                b.size -= new IntVector3(1);
                b.start += new IntVector3(1);
                foreach(var iv in b.IteratorXYZ)
                {
                    if(p.Contains(iv)) { continue; }
                    ivs.Add(iv);
                }
            }
            return ivs.ToArray();
        }

        static string FormatIntVector3Decl(IntVector3 i)
        {
            return string.Format("new IntVector3({0},{1},{2}) ", i.x, i.y, i.z);
        }
    }
}
