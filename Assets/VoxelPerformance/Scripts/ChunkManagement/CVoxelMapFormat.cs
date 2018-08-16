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

// VoxelPerformance/Scripts/VoxelMapFormat.cs
// Copyright 2016 Charles Griffiths

namespace Mel.VoxelGen
{
    // This class is a C# interface to VoxelPerformance/Shaders/MeshGeneration.compute
    public class CVoxelMapFormat
    {
        ComputeShader meshGen;

        int ExposedVoxelsKernel,
            ExposedVoxelsKernelLOD2,
            ExposedVoxelsKernelLOD4,
            FaceCopyKernel;

        // temporary buffers
        ComputeBuffer ShownVoxels;
        ComputeBuffer ShownVoxelsLOD2;
        ComputeBuffer ShownVoxelsLOD4;

        ComputeBuffer ShownVoxelCount, ShownVoxelOffset;
        ComputeBuffer TotalVoxelsPerLODCount;

        // passed to geometry shader
        ComputeBuffer SolidVoxels;  
        ComputeBuffer SolidVoxelsLOD2;
        ComputeBuffer SolidVoxelsLOD4;



        ComputeBuffer HilbertIndices;
        ComputeBuffer OutHilbertIndices;
        ComputeBuffer HilbertLODRanges;

        CVoxelMapData voxelMapData;
        VGenConfig vGenConfig;

        #region init 
        public CVoxelMapFormat(ComputeShader shader, ComputeShader hilbertIndexShader, CVoxelMapData data, VGenConfig vGenConfig)
        {
            meshGen = shader;

            this.vGenConfig = vGenConfig;

            initKernels();
            initTemporaryBuffers();
            voxelMapData = data;
            setTemporaryBuffers();

            SolidVoxels = null;
        }

        void initKernels()
        {
            ExposedVoxelsKernel = meshGen.FindKernel("ExposedVoxels");
            ExposedVoxelsKernelLOD2 = meshGen.FindKernel("ExposedVoxelsLOD2");
            ExposedVoxelsKernelLOD4 = meshGen.FindKernel("ExposedVoxelsLOD4");
            FaceCopyKernel = meshGen.FindKernel("FaceCopy");
            //ConstructHilbertIndicesKernel = hilbertIndexShader.FindKernel("ConstructHilbertIndices");
        }

        void initTemporaryBuffers()
        {

            ShownVoxels = new ComputeBuffer(vGenConfig.VoxelsPerChunk, sizeof(int), ComputeBufferType.Counter);
            ShownVoxelsLOD2 = new ComputeBuffer(vGenConfig.VoxelsPerChunkAtLOD(1), sizeof(int), ComputeBufferType.Counter);
            ShownVoxelsLOD4 = new ComputeBuffer(vGenConfig.VoxelsPerChunkAtLOD(2), sizeof(int), ComputeBufferType.Counter);

        }

        void setTemporaryBuffers()
        {
            meshGen.SetBuffer(ExposedVoxelsKernel, "MapVoxels", voxelMapData.MapVoxels);
            meshGen.SetBuffer(ExposedVoxelsKernelLOD2, "MapVoxelsLOD2", voxelMapData.MapVoxelsLOD2);
            meshGen.SetBuffer(ExposedVoxelsKernelLOD4, "MapVoxelsLOD4", voxelMapData.MapVoxelsLOD4);

            //meshGen.SetBuffer(ExposedVoxelsKernel, "ShownVoxelCount", ShownVoxelCount);
            meshGen.SetBuffer(ExposedVoxelsKernel, "ShownVoxels", ShownVoxels);
            meshGen.SetBuffer(ExposedVoxelsKernelLOD2, "ShownVoxelsLOD2", ShownVoxelsLOD2);
            meshGen.SetBuffer(ExposedVoxelsKernelLOD4, "ShownVoxelsLOD4", ShownVoxelsLOD4);


            meshGen.SetBuffer(FaceCopyKernel, "ShownVoxels", ShownVoxels);
            meshGen.SetBuffer(FaceCopyKernel, "ShownVoxelsLOD2", ShownVoxelsLOD2);
            meshGen.SetBuffer(FaceCopyKernel, "ShownVoxelsLOD4", ShownVoxelsLOD4);

            //Total counts
            //meshGen.SetBuffer(FaceCopyKernel, "TotalVoxelsPerLODCount", TotalVoxelsPerLODCount);

            //meshGen.SetBuffer(FaceCopyKernel, "ShownVoxelCount", ShownVoxelCount);
            //meshGen.SetBuffer(FaceCopyKernel, "ShownVoxelOffset", ShownVoxelOffset);
        }
        #endregion


        #region take-buffers

        public MapDisplay.LODBuffers takeDisplayBuffers()
        {
            var lodBuffers = new MapDisplay.LODBuffers();
            lodBuffers.display = SolidVoxels;
            lodBuffers.lod2 = SolidVoxelsLOD2;
            lodBuffers.lod4 = SolidVoxelsLOD4;

            SolidVoxels = null;
            SolidVoxelsLOD2 = null;
            SolidVoxelsLOD4 = null;
            return lodBuffers;
        }

        //public MapDisplay.DisplayBuffers takeDisplayBuffers()
        //{
        //    var dbuffer = new MapDisplay.DisplayBuffers();
        //    dbuffer.display = SolidVoxels;
        //    SolidVoxels = null;

        //    //
        //    // ifndef CPU side test
        //    //
        //    //dbuffer.hilbertIndices = OutHilbertIndices;
        //    //OutHilbertIndices = null;

        //    //dbuffer.hilbertLODRanges = HilbertLODRanges;
        //    //HilbertLODRanges = null;

        //    //
        //    // ifdef CPU side test
        //    //
        //    if (OutHilbertIndices != null)
        //        OutHilbertIndices.Release();
        //    dbuffer.hilbertIndices = new ComputeBuffer(lod2HindicesCPUSide.Count, sizeof(int));
        //    dbuffer.hilbertIndices.SetData(lod2HindicesCPUSide.ToArray());
        //    OutHilbertIndices = null;
        //    if (HilbertLODRanges != null)
        //        HilbertLODRanges.Release();
        //    HilbertLODRanges = null;
        //    uint[] lodRanges = new uint[] { (uint)dbuffer.display.count, (uint)lod2HindicesCPUSide.Count };



        //    dbuffer.hilbertLODRanges = new ComputeBuffer(lodRanges.Length, sizeof(uint));
        //    dbuffer.hilbertLODRanges.SetData(lodRanges);

        //    return dbuffer;
        //}

        #endregion


        #region buffer-counting
        public struct BufferCountArgs
        {
            public int[] args;
            public int count { get { return args[0]; } }

            public static BufferCountArgs FromBuffer(ComputeBuffer buffer)
            {
                int[] args = new int[] { 0, 1, 0, 0 };
                ComputeBuffer arbu = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
                arbu.SetData(args);
                ComputeBuffer.CopyCount(buffer, arbu, 0);
                arbu.GetData(args);

                arbu.Release();

                return new BufferCountArgs()
                {
                    args = args
                };
            }

            public static T[] GetData<T>(ComputeBuffer buffer)
            {
                if(buffer == null)
                {
                    return new T[0];
                }
                var bca = FromBuffer(buffer);
                //
                // Counter type buffers have a counter
                // Default type buffers dont; just use buffer.count
                //
                int count = bca.count;
                if(count == 0)
                {
                    count = buffer.count;
                }
                var data = new T[count];
                buffer.GetData(data);
                return data;
            }

            public static ComputeBuffer CreateBuffer<T>(T[] data)
            {
                var cb = new ComputeBuffer(data.Length, System.Runtime.InteropServices.Marshal.SizeOf(data[0]));
                cb.SetData(data);
                return cb;
            }

            public static ComputeBuffer CreateBuffer<T>(NativeArray<T> narray) where T : struct
            {
                if(narray.Length == 0) { return null; }
                T[] data = new T[narray.Length];
                narray.CopyTo(data);
                return CreateBuffer(data);
            }

            public static bool CreateBufferFromCount<T>(ComputeBuffer from, out ComputeBuffer target, ComputeBufferType type = ComputeBufferType.Default)
            {
                var args = FromBuffer(from);
                if(args.count == 0)
                {
                    target = null;
                    return false;
                }
                target = new ComputeBuffer(args.count, System.Runtime.InteropServices.Marshal.SizeOf(default(T)), type);
                return true;
            }

            public static string DebugArgsCountVsCount(ComputeBuffer buffer)
            {
                var bca = FromBuffer(buffer);
                return string.Format("Equal? {0} args count: {1}. count: {2}", bca.count == buffer.count, bca.count, buffer.count);
            }
        }

        public BufferCountArgs getBufferCount(ComputeBuffer buffer)
        {
            return BufferCountArgs.FromBuffer(buffer);
        }

        public int getShownVoxelCount()
        {
            return getBufferCount(ShownVoxels).count;
        }
        #endregion


        #region hilbert-sort

        public uint[] GetHilbertSortedVoxels()
        {
            int voxelCount = getShownVoxelCount();
            uint[] voxels = new uint[voxelCount];
            ShownVoxels.GetData(voxels, 0, 0, voxels.Length);

            int bits = vGenConfig.hilbertBits;
            return voxels.OrderBy(vox => HilbertTables.XYZToHilbertIndex[IntVector3.FromVoxelInt((int)vox).ToFlatXYZIndex(vGenConfig.ChunkSize)]).ToArray(); 
        }

        // TODO: hilbert sorting could be done on GPU while finding exposed voxels?
        public void hilbertSortVoxels()
        {
            uint[] hsorted = GetHilbertSortedVoxels();
            ShownVoxels.SetData(hsorted, 0, 0, hsorted.Length);

            SetLOD2HindicesCPUSide(hsorted);
        }

        List<int> lod2HindicesCPUSide;

        void SetLOD2HindicesCPUSide(uint[] hsorted)
        {
            if (hsorted.Length == 0) { return; }
            lod2HindicesCPUSide = new List<int>(hsorted.Length / 8);
            lod2HindicesCPUSide.Add(0);
            int prev = (int)hsorted[0];
            int next;
            for (int i = 0; i < hsorted.Length; ++i)
            {
                next = (int)hsorted[i];
                if (TestHilbertIndices.isDifferentHilbertCube(prev, next, 2))
                {
                    lod2HindicesCPUSide.Add(i);
                    prev = next;
                }
            }

            Debug.Log("hsorted: " + hsorted.Length);
            Debug.Log("LOD2 indices: " + lod2HindicesCPUSide.Count);
            List<int> lodTwoVoxels = new List<int>(lod2HindicesCPUSide.Count);
            foreach (int index in lod2HindicesCPUSide)
            {
                lodTwoVoxels.Add((int)hsorted[index]);
            }
            Debug.Log("lodTwoVoxels: " + lodTwoVoxels.Count);
            DebugDrawVoxelOrder.Draw(lodTwoVoxels.ToArray(), vGenConfig.ChunkSizeX + 1);
            //EditorApplication.isPaused = true;
        }

        #endregion


        #region call-kernels

        public void callFaceGenKernels()
        {
            ShownVoxels.SetCounterValue(0);
            meshGen.Dispatch(ExposedVoxelsKernel, vGenConfig.GroupsPerChunkX, vGenConfig.GroupsPerChunkY, vGenConfig.GroupsPerChunkZ); //  8, 1, 8 );

            ShownVoxelsLOD2.SetCounterValue(0);
            var groupsLOD2 = vGenConfig.GroupsPerChunkAtLOD(1);
            meshGen.Dispatch(ExposedVoxelsKernelLOD2, groupsLOD2.x, groupsLOD2.y, groupsLOD2.z);

            ShownVoxelsLOD4.SetCounterValue(0); 
            var groupsLOD4 = vGenConfig.GroupsPerChunkAtLOD(2);
            meshGen.Dispatch(ExposedVoxelsKernelLOD4, groupsLOD4.x, groupsLOD4.y, groupsLOD4.z);
        }

        public uint[] TestGetShownVoxelData()
        {
            int shCount = getShownVoxelCount();
            uint[] data = new uint[shCount];
            ShownVoxels.GetData(data, 0, 0, shCount);
            return data;
        }

        public uint[] TestGetSolidVoxelData()
        {
            uint[] data = new uint[SolidVoxels.count];
            Debug.Log("solid" + data.Length);
            SolidVoxels.GetData(data);
            return data;
        }

        

        //
        // Face copy kernels copy voxel data into packed arrays
        // Release each buffer reference (possibly pointing to data for which we are no longer responsible)
        // before assigning it to a new buffer.
        //
        public void callFaceCopyKernel()
        {
            int buffCreateError = 0;
            BufferUtil.ReleaseBuffers(SolidVoxels, SolidVoxelsLOD2, SolidVoxelsLOD4, TotalVoxelsPerLODCount);
            Assert.IsTrue(SolidVoxelsLOD2 == null, "Solide LOD 2 not null?");
            Assert.IsTrue(SolidVoxelsLOD4 == null, "Solide not null?");
            Assert.IsTrue(SolidVoxels == null, "Solide not null?");
            if (!BufferCountArgs.CreateBufferFromCount<uint>(ShownVoxels, out SolidVoxels))
            {
                buffCreateError++;
            }
            else
            {
                meshGen.SetBuffer(FaceCopyKernel, "SolidVoxels", SolidVoxels);
            }

            if (!BufferCountArgs.CreateBufferFromCount<uint>(ShownVoxelsLOD2, out SolidVoxelsLOD2))
            {
                buffCreateError++;
            }
            else
            {
                meshGen.SetBuffer(FaceCopyKernel, "SolidVoxelsLOD2", SolidVoxelsLOD2);
            }

            if (!BufferCountArgs.CreateBufferFromCount<uint>(ShownVoxelsLOD4, out SolidVoxelsLOD4))
            {
                buffCreateError++;
            }
            else
            {
                meshGen.SetBuffer(FaceCopyKernel, "SolidVoxelsLOD4", SolidVoxelsLOD4);
            }

            if(buffCreateError > 0)
            {
                BufferUtil.ReleaseBuffers(SolidVoxels, SolidVoxelsLOD2, SolidVoxelsLOD4, TotalVoxelsPerLODCount);
                //DBUG
                Debug.Log("Got buff create error: " + buffCreateError);
                //END DBUG
                return;
            }


            SetTotalsCountBuffer();
            meshGen.SetBuffer(FaceCopyKernel, "TotalVoxelsPerLODCount", TotalVoxelsPerLODCount);
            meshGen.Dispatch(FaceCopyKernel, 1, 1, 1);
        }

        private void SetTotalsCountBuffer()
        {
            var totals = new int[vGenConfig.NumLODLevels];
            totals[0] = BufferCountArgs.FromBuffer(ShownVoxels).count;
            totals[1] = BufferCountArgs.FromBuffer(ShownVoxelsLOD2).count;
            totals[2] = BufferCountArgs.FromBuffer(ShownVoxelsLOD4).count;

            BufferUtil.ReleaseBuffers(TotalVoxelsPerLODCount);
            TotalVoxelsPerLODCount = new ComputeBuffer(totals.Length, sizeof(int));
            TotalVoxelsPerLODCount.SetData(totals);
        }

        public void DebugBuffs()
        {

            //DebugChunkData.CheckUniqueData(ShownVoxels, "Shown unique?");
            //DebugChunkData.CheckUniqueData(ShownVoxelsLOD2, "LOD2 unique?");
            //DebugChunkData.CheckUniqueData(ShownVoxelsLOD4, "LOD4 unique?");

            //DebugChunkData.CheckAllSamePositionOnAxis(ShownVoxelsLOD4, 0, "ShownVoxLOD4");

        }

        #endregion




        public void releaseTemporaryBuffers()
        {
            ComputeBuffer[] buffs = new ComputeBuffer[]
            {
                ShownVoxelCount,
                ShownVoxelOffset,
                ShownVoxels,
                ShownVoxelsLOD2,
                ShownVoxelsLOD4,
                TotalVoxelsPerLODCount,
                HilbertIndices,
                OutHilbertIndices,
                HilbertLODRanges,
            };
            BufferUtil.ReleaseBuffers(buffs);

            //int i = 0;
            //foreach (var buff in buffs)
            //{
            //    if (buff == null)
            //    {
            //        Debug.Log("buff " + i + "was already null");
            //    }
            //    i++;
            //    releaseBuffer(buff);
            //}

        }

        //void releaseBuffer(ComputeBuffer buff)
        //{
        //    if (buff != null)
        //    {
        //        buff.Release();
        //    }
        //    buff = null;
        //}
    }
}

