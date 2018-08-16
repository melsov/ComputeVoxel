using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;

namespace Mel.VoxelGen
{
    public struct PFRaycast : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<uint> chunk;
        public NativeArray<uint> result;
        public Ray3f ray;
        public float maxDistance;
        public Bounds chunkBounds;

        public void Execute(int index)
        {
            // get first point inside bounds

            // iterate until we get a hit
            throw new NotImplementedException();
        }
    }

    public class PFOctRaycast
    {

    }
}
