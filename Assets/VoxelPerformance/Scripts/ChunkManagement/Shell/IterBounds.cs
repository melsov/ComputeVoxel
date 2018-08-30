using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mel.Math
{
    public static class IterBounds
    {


        public static IEnumerable<IntVector3> IteratorXZYWithNeighbors(IntBounds3 bounds)
        {
            foreach(var pos in bounds.IteratorXZYTopDown)
            {
                yield return pos;
                foreach(var nei in CubeNeighbors6.NeighborsOf(pos))
                {
                    if(!bounds.Contains(nei))
                    {
                        yield return nei;
                    }
                }
            }
        }

        public static IEnumerable<IntVector3> IteratorXZWithNeighborsAtY(IntBounds3 bounds, int y)
        {
            foreach(var pos in bounds.IteratorXZ)
            {
                var p = pos.ToIntVector3XZWithY(y);
                yield return p;
                foreach(var relEscape in CubeNeighbors6.EscapeFacesXZ(bounds.RelativeOrigin(p), bounds.size))
                {
                    yield return p + relEscape;
                }
            }
        }

        public static IEnumerable<IntVector3> IteratorXZAtY(IntBounds3 bounds, int y)
        {
            foreach(var pos in bounds.IteratorXZ)
            {
                yield return pos.ToIntVector3XZWithY(y);
            }
        }

        public static int AreaIncludingNeighborsXYZ(IntVector3 size)
        {
            return size.Area + 2 * size.x * size.z + 2 * size.x * size.y + 2 * size.z * size.y;
        }

        public static int AreaIncludingNeighborsXZ(IntVector2 size)
        {
            return size.Area + size.x * 2 + size.y * 2;
        }
    }
}
