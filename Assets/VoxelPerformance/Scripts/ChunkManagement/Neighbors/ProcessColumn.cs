using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mel.ChunkManagement;
using Mel.JobCallback;
using Mel.Math;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelPerformance;

namespace Mel.VoxelGen
{
    public class ProcessColumn : MonoBehaviour
    {

        VGenConfig _vg;
        public VGenConfig vGenConfig {
            get {
                if (!_vg) { _vg = GameObject.FindObjectOfType<VGenConfig>(); }
                return _vg;
            }
        }

        JobCall _jobCall;
        JobCall jobCall {
            get {
                if(!_jobCall)
                {
                    var go = new GameObject("JobCall-ProcessCol");
                    go.transform.SetParent(transform);
                    _jobCall = go.AddComponent<JobCall>();
                }
                return _jobCall;
            }
        }

        LocalLight localLight;

        private void Awake()
        {
            localLight = new LocalLight();
        }

        private void OnDestroy()
        {
            
        }


        internal async Task Process(ColumnAndHeightMap<NeighborChunkGenData> colAndHeightMap, Action<IntVector3> OnWroteChunkData) 
        {
            var keys = colAndHeightMap.column.Keys.OrderByDescending(a => a);

            foreach(var key in keys)
            {
                await ProcessSet(colAndHeightMap.column[key], OnWroteChunkData);
            }
        }


        async Task ProcessSet(NeighborChunkGenData neis, Action<IntVector3> OnWroteChunkData)
        {

            //TODO: trade data, etc...
            for (int i = 0; i < ChunkGenData.LODLevels ; ++i)
            {
                var cullJob = await CullHidden(neis, i);
                updateChunkCenterDisplay(cullJob, neis.centerChunkData, i);
                cullJob.DisposeAll();
            }

            await localLight.CalculateLocalLight(neis);

            // write center chunk
            var wroteAtPos = neis.center;

            //
            // TODO: write chunk at some other juncture
            //
            //var wroteAtPos = await SerializedChunkGenData.WriteAsync(neis.centerChunkData, SerializedChunkGenData.GenDataFullPath(neis.center));

            // write center chunk display arrays
            SerializedChunk.SerializedDisplayBuffers.WriteLODArrays(neis.centerChunkData.displays, neis.center);

            Chunk.MetaData metaData = new Chunk.MetaData();
            metaData.LODBufferLengths = neis.centerChunkData.displays.getLengths();
            metaData.HasBeenNeighborProcessed = true;
            XMLOp.Serialize(metaData, SerializedChunk.GetMetaDataFullPath(neis.center));
            
            OnWroteChunkData(wroteAtPos);
        }

        public static void DebugCheckGenDataHasEmpties(ChunkGenData data, string msg = "", uint EmptyVoxel = 0)
        {
            var ray = data.lods[0];
            int gotAZero = 0;
            foreach(var vox in ray)
            {
                if(vox.voxel == EmptyVoxel)
                {
                    gotAZero++;
                }
            }

            Debug.Log(msg + " got a zero: " + gotAZero + " out of " + ray.Length);
        }

        void updateChunkCenterDisplay( CullHiddenEdgeVoxelsJob cullJob, ChunkGenData data, int lod = 0)
        {
            var display = new List<VoxelGeomDataMirror>(data.displays.lodArrays[lod].Length);
            uint emptyVoxel = vGenConfig.EmptyVoxel;
            for(int i =0; i<cullJob.displayVoxels.Length; ++i)
            {
                if(cullJob.displayVoxels[i].voxel != emptyVoxel)
                {
                    display.Add(cullJob.displayVoxels[i]);
                }
            }
            data.displays.lodArrays[lod] = display.ToArray();
        }

        async Task<CullHiddenEdgeVoxelsJob> CullHidden(NeighborChunkGenData neis, int lodIndex = 0)
        {
            var allo = Allocator.TempJob;
            var job = SetupCullHidden(neis, allo, lodIndex);
            await jobCall.ScheduleParallelAwaitable(job, neis.centerChunkData.displays.lodArrays[lodIndex].Length, 1, allo);
            return job;
        }

        CullHiddenEdgeVoxelsJob SetupCullHidden(NeighborChunkGenData neis, Allocator _alloc = Allocator.TempJob, int lodIndex = 0)
        {
            CullHiddenEdgeVoxelsJob job = new CullHiddenEdgeVoxelsJob();
            job.displayVoxels = new NativeArray<VoxelGeomDataMirror>(neis.centerChunkData.displays[lodIndex], _alloc);


            job.center = new NativeArray<VoxelGenDataMirror>(neis.centerChunkData.lods[lodIndex], _alloc);

            // TODO: neibs always get the full chunk data
            // have to rewrite some of parts of the get voxel at function
            // when lod gr than 0. have to search multiple nei cubes (we think)

            job.right = new NativeArray<VoxelGenDataMirror>(neis.Get(NeighborDirection.Right).lods[lodIndex], _alloc);
            job.left = new NativeArray<VoxelGenDataMirror>(neis.Get(NeighborDirection.Left).lods[lodIndex], _alloc);
            job.up = new NativeArray<VoxelGenDataMirror>(neis.Get(NeighborDirection.Up).lods[lodIndex], _alloc);
            job.down = new NativeArray<VoxelGenDataMirror>(neis.Get(NeighborDirection.Down).lods[lodIndex], _alloc);
            job.forward = new NativeArray<VoxelGenDataMirror>(neis.Get(NeighborDirection.Forward).lods[lodIndex], _alloc);
            job.back = new NativeArray<VoxelGenDataMirror>(neis.Get(NeighborDirection.Back).lods[lodIndex], _alloc);

            job.chunkSize = vGenConfig.ChunkSize;
            job.voxelsPerMapData = VGenConfig.VoxelsPerMapData;
            job.EmptyVoxel = vGenConfig.EmptyVoxel;
            job.lodIndex = lodIndex;

            return job;
        }



        struct CullHiddenEdgeVoxelsJob : IJobParallelFor
        {

            public NativeArray<VoxelGeomDataMirror> displayVoxels;

            [ReadOnly] public NativeArray<VoxelGenDataMirror> center;
            [ReadOnly] public NativeArray<VoxelGenDataMirror> right;
            [ReadOnly] public NativeArray<VoxelGenDataMirror> left;
            [ReadOnly] public NativeArray<VoxelGenDataMirror> up;
            [ReadOnly] public NativeArray<VoxelGenDataMirror> down;
            [ReadOnly] public NativeArray<VoxelGenDataMirror> forward;
            [ReadOnly] public NativeArray<VoxelGenDataMirror> back;

            [ReadOnly] public IntVector3 chunkSize;
            [ReadOnly] public int voxelsPerMapData;
            [ReadOnly] public uint EmptyVoxel;
            [ReadOnly] public int lodIndex;

            public void DisposeAll()
            {
                var rays = new NativeArray<VoxelGenDataMirror>[]
                {
                    center, right, left, up, down, forward, back
                };
                for(int i = 0; i < rays.Length; ++i)
                {
                    if(rays[i].IsCreated)
                        rays[i].Dispose();
                }
                if(displayVoxels.IsCreated)
                    displayVoxels.Dispose();
            }

            public NativeArray<VoxelGenDataMirror> get(NeighborDirection nd)
            {
                switch (nd)
                {
                    case NeighborDirection.Right: default: return right;
                    case NeighborDirection.Left: return left;
                    case NeighborDirection.Up: return up;
                    case NeighborDirection.Down: return down;
                    case NeighborDirection.Forward: return forward;
                    case NeighborDirection.Back: return back;
                }
            }

            uint GetVoxelFromGenDataArray(NativeArray<VoxelGenDataMirror> data, IntVector3 posOverDiv)
            {
                int opI;
                if(lodIndex == 0)
                {
                    opI = posOverDiv.ToFlatZXYIndex(chunkSize);
                    int opMod = opI % voxelsPerMapData;
                    opI /= voxelsPerMapData;
                    uint voxels = data[opI].voxel;
                    return (uint)VGenConfig.DecodeMapGenType(voxels, opMod);

                } else
                {
                    int div = (int)Mathf.Pow(2, lodIndex);
                    opI = (posOverDiv).ToFlatXYZIndex(chunkSize / div);
                    return (uint)VGenConfig.DecodeVoxelType(data[opI].voxel); //This is a type-x-y-z encoded voxel if set. if not, its just EmptyVoxel
                }

            }


            public void Execute(int index)
            {
                var pos = IntVector3.FromUint256(displayVoxels[index].voxel);
                bool hidden = false;
                int div = (int)Mathf.Pow(2, lodIndex);
                var touchDirections = CubeNeighbors6.TouchFaces(pos / div, chunkSize / div).ToList();
                foreach (var nd in touchDirections)
                {
                    hidden = true;
                    var oppDir = CubeNeighbors6.Opposite(nd);
                    var opp = CubeNeighbors6.SnapToFace(pos/div, chunkSize / div, oppDir);
                    uint voxel = GetVoxelFromGenDataArray(get(nd), opp);

                    if(voxel == EmptyVoxel)
                    {
                        hidden = false;
                        break;
                    }
                }
                if (hidden)
                {
                    foreach(var nd in CubeNeighbors6.Directions)
                    {
                        if(touchDirections.Contains(nd)) { continue; }

                        var nudge = pos / div + CubeNeighbors6.Relative(nd);
                        uint voxel = GetVoxelFromGenDataArray(center, nudge);

                        if(voxel == EmptyVoxel) 
                        {
                            hidden = false;
                            break;
                        }
                    }
                }

                if (hidden)
                {
                    var geomData = displayVoxels[index];
                    geomData.voxel = EmptyVoxel;
                    displayVoxels[index] = geomData;
                } 
            }
        }
    }
}
