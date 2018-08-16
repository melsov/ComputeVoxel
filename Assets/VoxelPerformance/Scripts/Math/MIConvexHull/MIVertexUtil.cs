using UnityEngine;

namespace MIConvexHull
{
    public static class MIVertexUtil
    {
        public static IVertex toIVertex(this Vector3 v)
        {
            return new DefaultVertex() { Position = new double[] { v.x, v.y, v.z } };
        }

        public static Vector3 toVector3(this double[] u3)
        {
            return new Vector3((float)u3[0], (float)u3[1], (float)u3[2]);
        }

        public static double[] VoxelIntToDouble3(int voxel)
        {
            return new double[] { (voxel >> 16) & 0xFF, (voxel >> 8) & 0xFF, voxel & 0xFF }; 
        }
    }
}
