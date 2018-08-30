using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mel.VoxelGen;
using HilbertExtensions;
using System.Linq;
using Mel.Math;
using MIConvexHull;
using Mel.Util;
using Mel.Editorr;
using UnityEditor;
using VoxelPerformance;
using UnityEngine.Assertions;
using Unity.Collections;

namespace Mel.VoxelGen
{
    public class CVoxelFaceCopy
    {

        ComputeShader faceCopy;

        int FaceCopyKernel;


        // passed to geometry shader
        ComputeBuffer SolidVoxels;
        ComputeBuffer SolidVoxelsLOD2;
        ComputeBuffer SolidVoxelsLOD4;

        ComputeBuffer TotalVoxelsPerLODCount;


        public void Release()
        {
            ComputeBuffer[] buffs = new ComputeBuffer[]
            {
                TotalVoxelsPerLODCount,
                SolidVoxels,
                SolidVoxelsLOD2,
                SolidVoxelsLOD4
            };
            BufferUtil.ReleaseBuffers(buffs);

        }

        CVoxelNeighborFormat cvoxelNeighborFormat;
        VGenConfig vGenConfig;

        #region init 

        public CVoxelFaceCopy(ComputeShader shader, CVoxelNeighborFormat cvoxelNeighborFormat, VGenConfig vGenConfig)
        {
            faceCopy = shader;
            this.vGenConfig = vGenConfig;
            this.cvoxelNeighborFormat = cvoxelNeighborFormat;

            initKernels();
            initTemporaryBuffers();
            setTemporaryBuffers();
        }

        void initKernels()
        {
            FaceCopyKernel = faceCopy.FindKernel("FaceCopy");
        }

        void initTemporaryBuffers()
        {
            //
            // Init once reset counter before each call
            //
              
            int sizeOfVoxelGeom = System.Runtime.InteropServices.Marshal.SizeOf(new VoxelGeomDataMirror());
            SolidVoxels = new ComputeBuffer(vGenConfig.VoxelsPerChunk, sizeOfVoxelGeom, ComputeBufferType.Counter);
            SolidVoxelsLOD2 = new ComputeBuffer(vGenConfig.VoxelsPerChunkAtLOD(1), sizeOfVoxelGeom, ComputeBufferType.Counter);
            SolidVoxelsLOD4 = new ComputeBuffer(vGenConfig.VoxelsPerChunkAtLOD(2), sizeOfVoxelGeom, ComputeBufferType.Counter);
            TotalVoxelsPerLODCount = new ComputeBuffer(ChunkGenData.LODLevels, sizeof(int));
            
        }

        void setTemporaryBuffers()
        {
            faceCopy.SetBuffer(FaceCopyKernel, "ShownVoxels", cvoxelNeighborFormat.ShownVoxels);
            faceCopy.SetBuffer(FaceCopyKernel, "ShownVoxelsLOD2", cvoxelNeighborFormat.ShownVoxelsLOD2);
            faceCopy.SetBuffer(FaceCopyKernel, "ShownVoxelsLOD4", cvoxelNeighborFormat.ShownVoxelsLOD4);

            //WANT
            faceCopy.SetBuffer(FaceCopyKernel, "SolidVoxels", SolidVoxels);
            faceCopy.SetBuffer(FaceCopyKernel, "SolidVoxelsLOD2", SolidVoxelsLOD2);
            faceCopy.SetBuffer(FaceCopyKernel, "SolidVoxelsLOD4", SolidVoxelsLOD4);
        }

        #endregion

        #region take-buffers

        public void DebugSetSolids(ChunkGenData data)
        {
            BufferUtil.ReleaseBuffers(SolidVoxels, SolidVoxelsLOD2, SolidVoxelsLOD4);
            for(int i = 0; i < data.displays.lodArrays.Length; ++i)
            {
                DebugSetSolidAt(i, data.displays.lodArrays[i]);
            }
        }

        void DebugSetSolidAt(int i, VoxelGeomDataMirror[] data)
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(new VoxelGeomDataMirror());
            var buff = new ComputeBuffer(data.Length, size, ComputeBufferType.Counter);
            buff.SetData(data);
            buff.SetCounterValue((uint)data.Length);
            SetSolidAtDBUG(i, buff);
        }

        void SetSolidAtDBUG(int i, ComputeBuffer buff)
        {
            switch (i)
            {
                case 0: SolidVoxels = buff; break;
                case 1: SolidVoxelsLOD2 = buff; break;
                case 2: SolidVoxelsLOD4 = buff; break;
                default: break;
            }
        }

        ComputeBuffer GetSolidAtLOD(int i)
        {
            switch (i)
            {
                case 0: return SolidVoxels;
                case 1: return SolidVoxelsLOD2;
                case 2: return SolidVoxelsLOD4;
                default: return null;
            }
        }

        public MapDisplay.LODArrays CopySolidArraysUsingCounts()
        {
            var lod = MapDisplay.LODArrays.Create();
            for (int i = 0; i < ChunkGenData.LODLevels; ++i)
            {
                VoxelGeomDataMirror[] data;
                CVoxelMapFormat.BufferCountArgs.GetCounterBufferData(GetSolidAtLOD(i), out data);
                lod[i] = data;
            }
            return lod;
        }

        #endregion

        #region call-kernels

        public void callFaceCopyKernel()
        {
            SolidVoxels.SetCounterValue(0);
            SolidVoxelsLOD2.SetCounterValue(0);
            SolidVoxelsLOD4.SetCounterValue(0);

            SetTotalsCountBuffer();
            faceCopy.SetBuffer(FaceCopyKernel, "TotalVoxelsPerLODCount", TotalVoxelsPerLODCount);
            faceCopy.Dispatch(FaceCopyKernel, 1, 1, 1);
        }

        //
        // Face copy kernels copy voxel data into packed arrays
        // Release each buffer reference (possibly pointing to data for which we are no longer responsible)
        // before assigning it to a new buffer.
        //
        //public void callFaceCopyKernel()
        //{
        //    int buffCreateError = 0;
        //    BufferUtil.ReleaseBuffers(SolidVoxels, SolidVoxelsLOD2, SolidVoxelsLOD4, TotalVoxelsPerLODCount);

        //    if (!CVoxelMapFormat. BufferCountArgs.CreateBufferFromCount<VoxelGeomDataMirror>(ShownVoxels, out SolidVoxels))
        //    {
        //        buffCreateError++;
        //    }
        //    else
        //    {
        //        faceCopy.SetBuffer(FaceCopyKernel, "SolidVoxels", SolidVoxels);
        //    }

        //    if (!CVoxelMapFormat.BufferCountArgs.CreateBufferFromCount<VoxelGeomDataMirror>(ShownVoxelsLOD2, out SolidVoxelsLOD2))
        //    {
        //        buffCreateError++;
        //    }
        //    else
        //    {
        //        faceCopy.SetBuffer(FaceCopyKernel, "SolidVoxelsLOD2", SolidVoxelsLOD2);
        //    }

        //    if (!CVoxelMapFormat.BufferCountArgs.CreateBufferFromCount<VoxelGeomDataMirror>(ShownVoxelsLOD4, out SolidVoxelsLOD4))
        //    {
        //        buffCreateError++;
        //    }
        //    else
        //    {
        //        faceCopy.SetBuffer(FaceCopyKernel, "SolidVoxelsLOD4", SolidVoxelsLOD4);
        //    }

        //    if (buffCreateError > 0)
        //    {
        //        BufferUtil.ReleaseBuffers(SolidVoxels, SolidVoxelsLOD2, SolidVoxelsLOD4, TotalVoxelsPerLODCount);
        //        //DBUG
        //        Debug.Log("Got buff create error: " + buffCreateError);
        //        //END DBUG
        //        return;
        //    }


        //    SetTotalsCountBuffer();
        //    faceCopy.SetBuffer(FaceCopyKernel, "TotalVoxelsPerLODCount", TotalVoxelsPerLODCount);
        //    faceCopy.Dispatch(FaceCopyKernel, 1, 1, 1);
        //}

        private void SetTotalsCountBuffer()
        {
            var totals = new int[vGenConfig.NumLODLevels];
            totals[0] = CVoxelMapFormat.BufferCountArgs.FromBuffer(cvoxelNeighborFormat.ShownVoxels).count;
            totals[1] = CVoxelMapFormat.BufferCountArgs.FromBuffer(cvoxelNeighborFormat.ShownVoxelsLOD2).count;
            totals[2] = CVoxelMapFormat.BufferCountArgs.FromBuffer(cvoxelNeighborFormat.ShownVoxelsLOD4).count;

            BufferUtil.ReleaseBuffers(TotalVoxelsPerLODCount);
            TotalVoxelsPerLODCount = new ComputeBuffer(totals.Length, sizeof(int));
            TotalVoxelsPerLODCount.SetData(totals);
        }


        #endregion

        public void DebugBuffs()
        {
        }


    }
}

