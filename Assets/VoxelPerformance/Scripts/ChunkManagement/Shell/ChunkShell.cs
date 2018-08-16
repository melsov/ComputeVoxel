using System;
using System.Collections.Generic;
using System.Linq;
using Mel.Math;
using Mel.Storage;
using Unity.Collections;
using Unity.Jobs;

namespace Mel.VoxelGen
{
    public struct ChunkShell
    {
        public FlatArray3D<VoxelGenDataMirror> contents;
        public FlatArray2D<VoxelGenDataMirror>[] shell;

        public struct GenContentsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<VoxelGenDataMirror> chunkData;
            public NativeArray<VoxelGenDataMirror> contents;
            [ReadOnly] public IntVector3 chunkSize;

            public void Execute(int index)
            {
                var voxel = chunkData[index];
                IntVector3 v = IntVector3.FromUint256((int)voxel.voxel);
                int flatIndex = v.ToFlatXYZIndex(chunkSize);
                contents[flatIndex] = voxel;
            }
        }
    }
}
