using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Mel.Math;
using System.Threading;
using UnityEngine;
using Mel.JobCallback;

namespace Mel.VoxelGen
{
    public class LocalLight 
    {

        VGenConfig _vg;
        VGenConfig vGenConfig {
            get {
                if(!_vg)
                {
                    _vg = GameObject.FindObjectOfType<VGenConfig>();
                }
                return _vg;
            }
        }

        JobCall _jobCall;
        JobCall jobCall {
            get {
                if (!_jobCall)
                {
                    var go = new GameObject("JobCall-LocalLight");
                    _jobCall = go.AddComponent<JobCall>();
                }
                return _jobCall;
            }
        }

        public async Task CalculateLocalLight(NeighborChunkGenData nei)
        {
            // yes, this is a native array or 27 nativearrays or uints
            var job = new VertexLocalLightJob();
            Allocator allo = Allocator.TempJob;
            //var cubeOfExists = NativeNeighbors27<NativeArray<uint>>.FromNeighbor27(nei.nieghbors, allo);  new Neighbors27<NativeArray<uint>>(nei.center), allo);
            SetUp(nei, job, 0, allo);
            await jobCall.ScheduleParallelAwaitable(job, job.display.Length, 1, allo);
            job.DisposeAll();
        }

        private void SetUp(NeighborChunkGenData nei,  VertexLocalLightJob job, int lodIndex, Allocator allo)
        {
            job.locks = new object[nei.centerChunkData.displays.getLengths()[lodIndex]];
            job.display = new NativeArray<VoxelGeomDataMirror>(nei.centerChunkData.displays[lodIndex], allo);
            job.chunkSize = vGenConfig.ChunkSize;
            job.SizeOfHLSLUInt = VGenConfig.SizeOfHLSLInt;

            var cubeOfExists27 = new  NativeNeighbors27<NativeArray<uint>>(nei.center, allo);

            foreach(var pos in Cube27.NeighborsOfIncludingCenter(nei.center))
            {
                cubeOfExists27.Set(pos, new NativeArray<uint>(nei.Get(pos).ExistsMap, allo));
            }

            job.cubeOfExists = cubeOfExists27;
        }

        public struct VertexLocalLightJob : IJobParallelFor
        {
            [ReadOnly] public NativeNeighbors27<NativeArray<uint>> cubeOfExists;
            [NativeMatchesParallelForLength] public NativeArray<VoxelGeomDataMirror> display;
            [NativeMatchesParallelForLength] public object[] locks;

            public IntVector3 chunkSize;
            public int SizeOfHLSLUInt;

            public void DisposeAll()
            {
                display.Dispose();
                foreach (var na in cubeOfExists.Iterator)
                {
                    na.Dispose();
                }

            }

            uint ExistsAt(IntVector3 voxPos)
            {
                IntVector3 neighborChunkKey;
                var index3 = Lookup27.ChunkIndex(voxPos, chunkSize, out neighborChunkKey);
                var existsBuffer = cubeOfExists[neighborChunkKey];
                return ChunkIndex.GetExists(existsBuffer, index3, chunkSize);
            }

            public void Execute(int index)
            {
                IntVector3 pos = IntVector3.FromUint256(display[index].voxel);

                lock (locks[index])
                {
                    foreach (var relative in CrossNeighbors12.members)
                    {
                        var neiPos = pos + relative;
                        if (ExistsAt(neiPos) == 1)
                        {
                            //todo: perform update
                            var Geom = display[index];
                            Geom.extras = CrossBits12.SetBit(relative, true, Geom.extras);
                            display[index] = Geom;
                        }
                    }
                }

            }

        }

    }
}
