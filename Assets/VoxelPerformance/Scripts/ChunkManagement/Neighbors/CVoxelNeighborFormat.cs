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
using System.Text;

namespace Mel.VoxelGen
{
    //public struct BufferLODSet
    //{
    //    public ComputeBuffer[] lods;

    //    public BufferLODSet(int count)
    //    {
    //        lods = new ComputeBuffer[count];
    //    }

    //    public ComputeBuffer this[int i] {
    //        get {
    //            return lods[i];
    //        }
    //        set {
    //            lods[i] = value;
    //        }
    //    }
    //}

    public class CVoxelNeighborFormat
    {
        #region mem-vars

        ComputeShader neighborFormat;

        int ExposedVoxelsKernel,
            ExposedVoxelsKernelLOD2,
            ExposedVoxelsKernelLOD4;

        // temporary buffers
        ComputeBuffer MapVoxels { get;  set; }
        ComputeBuffer MapVoxelsLOD2 { get;  set; }
        ComputeBuffer MapVoxelsLOD4 { get;  set; }

        public ComputeBuffer ShownVoxels { get; private set; }
        public ComputeBuffer ShownVoxelsLOD2 { get; private set; }
        public ComputeBuffer ShownVoxelsLOD4 { get; private set; }

        ComputeBuffer ExistsMap27;


        public void Release()
        {
            BufferUtil.ReleaseBuffers(
                MapVoxels,
                MapVoxelsLOD2,
                MapVoxelsLOD4,
                ExistsMap27,
                ShownVoxels,
                ShownVoxelsLOD2,
                ShownVoxelsLOD4
                );
        }

        VGenConfig vGenConfig;

        #endregion
        
        // Kernel fills ShownVoxels 

        // Face copy kernel can run in a group (of size ShownVoxels.count)
        // And use an interlocked increment


        #region init

        public CVoxelNeighborFormat(ComputeShader neighborFormat, VGenConfig vGenConfig)
        {
            this.neighborFormat = neighborFormat;
            this.vGenConfig = vGenConfig;

            InitKernels();
            initBuffers();
            SetKernelBuffers();
        }

        void InitKernels()
        {
            ExposedVoxelsKernel = neighborFormat.FindKernel("ExposedVoxels");
            ExposedVoxelsKernelLOD2 = neighborFormat.FindKernel("ExposedVoxelsLOD2");
            ExposedVoxelsKernelLOD4 = neighborFormat.FindKernel("ExposedVoxelsLOD4");
        }

        void SetKernelBuffers()
        {
            neighborFormat.SetBuffer(ExposedVoxelsKernel, "MapVoxels", MapVoxels);
            neighborFormat.SetBuffer(ExposedVoxelsKernelLOD2, "MapVoxelsLOD2", MapVoxelsLOD2);
            neighborFormat.SetBuffer(ExposedVoxelsKernelLOD4, "MapVoxelsLOD4", MapVoxelsLOD4);

            neighborFormat.SetBuffer(ExposedVoxelsKernel, "ShownVoxels", ShownVoxels);
            neighborFormat.SetBuffer(ExposedVoxelsKernelLOD2, "ShownVoxelsLOD2", ShownVoxelsLOD2);
            neighborFormat.SetBuffer(ExposedVoxelsKernelLOD4, "ShownVoxelsLOD4", ShownVoxelsLOD4);

            neighborFormat.SetBuffer(ExposedVoxelsKernel, "ExistsMap27", ExistsMap27);
            neighborFormat.SetBuffer(ExposedVoxelsKernelLOD2, "ExistsMap27", ExistsMap27);
            neighborFormat.SetBuffer(ExposedVoxelsKernelLOD4, "ExistsMap27", ExistsMap27);
        }

        void initBuffers()
        {
            MapVoxels = new ComputeBuffer(vGenConfig.ChunkPerlinGenArraySize, sizeof(uint));
            MapVoxelsLOD2 = new ComputeBuffer(vGenConfig.VoxelsPerChunk / 8, sizeof(uint));
            MapVoxelsLOD4 = new ComputeBuffer(vGenConfig.VoxelsPerChunk / 64, sizeof(uint));

            int ShownSize = System.Runtime.InteropServices.Marshal.SizeOf(new VoxelGeomDataMirror());

            ShownVoxels = new ComputeBuffer(vGenConfig.VoxelsPerChunk, ShownSize, ComputeBufferType.Counter);
            ShownVoxelsLOD2 = new ComputeBuffer(vGenConfig.VoxelsPerChunkAtLOD(1), ShownSize, ComputeBufferType.Counter);
            ShownVoxelsLOD4 = new ComputeBuffer(vGenConfig.VoxelsPerChunkAtLOD(2), ShownSize, ComputeBufferType.Counter);

            ExistsMap27 = new ComputeBuffer(vGenConfig.SizeOfExistsMap * 27, sizeof(uint));
        }

        #endregion

        #region set-buffers

        public void SetBuffersWith(NeighborChunkGenData nei)
        {
            SetMapVoxels(nei);
            SetExistsMap(nei);
        }

        void SetMapVoxels(NeighborChunkGenData nei)
        {
            

            MapVoxels.SetData(nei.centerChunkData.lods[0]);
            MapVoxelsLOD2.SetData(nei.centerChunkData.lods[1]);
            MapVoxelsLOD4.SetData(nei.centerChunkData.lods[2]);
        }


        void SetExistsMap(NeighborChunkGenData nei)
        {
            int sizeOfExistMap = vGenConfig.SizeOfExistsMap;
            var bounds = nei.neighbors.bounds;

            foreach(var pos in bounds.IteratorXYZ)
            {
                var rel = bounds.RelativeOrigin(pos);
                int start = rel.ToFlatZXYIndex(new IntVector3(3));

                ExistsMap27.SetData(nei.Get(pos).ExistsMap, 0, start * sizeOfExistMap, sizeOfExistMap);
            }
        }

        bool TestIsAllOnes(uint i)
        {
            Debug.Log("size of uint: " + sizeof(uint));
            int zeros = 0;
            for(int j = 0; j < sizeof(uint) * 8; ++j)
            {
                int m = ((int)i >> j) & 1;
                if(m == 0)
                {
                    zeros++;
                }
            }
            if(zeros != 0)
            {
                Debug.Log("no. this many zeros: " + zeros);
            }
            return zeros == 0;
        }

        private void TestExistMapDebug(uint[] existsMap)
        {
            StringBuilder s = new StringBuilder();
            for(int i = 0; i < 20; ++i)
            {
                s.Append(Convert.ToString(existsMap[i], 2));
                s.Append("," + Environment.NewLine);
                //var pass = TestIsAllOnes(existsMap[i]);
            }
            Debug.Log(s.ToString());
        }

        #endregion

        #region call-kernels

        public void callNeighborFormatKernels()
        {
            ShownVoxels.SetCounterValue(0);
            neighborFormat.Dispatch(ExposedVoxelsKernel, vGenConfig.GroupsPerChunkX, vGenConfig.GroupsPerChunkY, vGenConfig.GroupsPerChunkZ); 

            ShownVoxelsLOD2.SetCounterValue(0);
            var groupsLOD2 = vGenConfig.GroupsPerChunkAtLOD(1);
            neighborFormat.Dispatch(ExposedVoxelsKernelLOD2, groupsLOD2.x, groupsLOD2.y, groupsLOD2.z);

            ShownVoxelsLOD4.SetCounterValue(0);
            var groupsLOD4 = vGenConfig.GroupsPerChunkAtLOD(2);
            neighborFormat.Dispatch(ExposedVoxelsKernelLOD4, groupsLOD4.x, groupsLOD4.y, groupsLOD4.z);
        }

        #endregion
    }
}
