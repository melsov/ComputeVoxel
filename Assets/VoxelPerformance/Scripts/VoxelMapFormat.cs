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

// VoxelPerformance/Scripts/VoxelMapFormat.cs
// Copyright 2016 Charles Griffiths

namespace VoxelPerformance
{
    // This class is a C# interface to VoxelPerformance/Shaders/MeshGeneration.compute
    public class VoxelMapFormat
    {
        ComputeShader meshGen;
        ComputeShader hilbertIndexShader;
        int ExposedVoxelsKernel, 
            //FaceSumKernel, 
            FaceCopyKernel, GetFacesKernel;
        //int ExposedMip64Kernel;
        int ConstructHilbertIndicesKernel;

        ComputeBuffer ShownVoxels;
        ComputeBuffer ShownVoxelCount, ShownVoxelOffset;
        ComputeBuffer TotalVoxelCount; // temporary buffers
        ComputeBuffer SolidVoxels;  // passed to geometry shader
        ComputeBuffer argBuffer;

        ComputeBuffer HilbertIndices;
        ComputeBuffer OutHilbertIndices;
        ComputeBuffer HilbertLODRanges;

        VoxelMapData voxelMapData;
        VGenConfig vGenConfig;

        #region init
        public VoxelMapFormat(ComputeShader shader, ComputeShader hilbertIndexShader, VoxelMapData data, VGenConfig vGenConfig) {
            meshGen = shader;
            this.hilbertIndexShader = hilbertIndexShader;
            this.vGenConfig = vGenConfig;

            initKernels();
            initTemporaryBuffers();
            voxelMapData = data;
            setTemporaryBuffers();

            SolidVoxels = null;
        }

        void initKernels() {
            ExposedVoxelsKernel = meshGen.FindKernel("ExposedVoxels");
            FaceCopyKernel = meshGen.FindKernel("FaceCopy");
            GetFacesKernel = meshGen.FindKernel("GetFaces");
            //ExposedMip64Kernel = meshGen.FindKernel("ExposedMip64");
            ConstructHilbertIndicesKernel = hilbertIndexShader.FindKernel("ConstructHilbertIndices");
        }

        void initTemporaryBuffers() {
            ShownVoxelCount = new ComputeBuffer(vGenConfig.ColumnsPerChunk, sizeof(int));
            ShownVoxelOffset = new ComputeBuffer(vGenConfig.ColumnsPerChunk, sizeof(int));
            ShownVoxels = new ComputeBuffer(vGenConfig.VoxelsPerChunk, sizeof(int), ComputeBufferType.Counter);
            argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            TotalVoxelCount = new ComputeBuffer(1, sizeof(int));
        }
        #endregion


        #region take-buffers

        public MapDisplay.DisplayBuffers takeDisplayBuffers()
        {
            var dbuffer = new MapDisplay.DisplayBuffers();
            dbuffer.display = SolidVoxels;
            SolidVoxels = null;

            //
            // ifndef CPU side test
            //
            //dbuffer.hilbertIndices = OutHilbertIndices;
            //OutHilbertIndices = null;

            //dbuffer.hilbertLODRanges = HilbertLODRanges;
            //HilbertLODRanges = null;

            //
            // ifdef CPU side test
            //
            if (OutHilbertIndices != null)
                OutHilbertIndices.Release();
            dbuffer.hilbertIndices = new ComputeBuffer(lod2HindicesCPUSide.Count, sizeof(int));
            dbuffer.hilbertIndices.SetData(lod2HindicesCPUSide.ToArray());
            OutHilbertIndices = null;
            if (HilbertLODRanges != null)
                HilbertLODRanges.Release();
            HilbertLODRanges = null;
            uint[] lodRanges = new uint[] { (uint)dbuffer.display.count, (uint)lod2HindicesCPUSide.Count };



            dbuffer.hilbertLODRanges = new ComputeBuffer(lodRanges.Length, sizeof(uint));
            dbuffer.hilbertLODRanges.SetData(lodRanges);

            return dbuffer;
        }

        #endregion

        public void releaseSolidVoxels() {
            if (null != SolidVoxels)
                SolidVoxels.Release();
            SolidVoxels = null;
        }

        public void releaseOutHilbert()
        {
            if(OutHilbertIndices != null) { OutHilbertIndices.Release(); }
            OutHilbertIndices = null;
        }

        public void releaseHilbert()
        {
            if(HilbertIndices != null) { HilbertIndices.Release(); }
            HilbertIndices = null;
        }



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
        // TODO: hilbert sorting could be done on GPU while finding exposed voxels?
        public void hilbertSortVoxels() {
            int voxelCount = getShownVoxelCount();
            int[] voxels = new int[voxelCount];
            ShownVoxels.GetData(voxels, 0, 0, voxels.Length);


            List<IntVoxel3> voxVs = new List<IntVoxel3>(voxels.Length);
            foreach(int voxel in voxels)
            {
                voxVs.Add(new IntVoxel3(voxel, 0));
            }

            int bits = vGenConfig.hilbertBits;
            int[] hsorted = voxels.OrderBy(vox => IntVector3.FromVoxelInt(vox).ToUint3().CoordsToFlatHilbertIndex(bits)).ToArray();
            //IntVector3.FromVoxelInt(vox).ToUint3().HilbertIndexTransposed(bits).FlatHilbertIndex(bits)).ToArray();

            ShownVoxels.SetData(hsorted, 0, 0, hsorted.Length);

            //
            //
            SetLOD2HindicesCPUSide(hsorted);
        }

        List<int> lod2HindicesCPUSide;

        void SetLOD2HindicesCPUSide(int[] hsorted)
        {
            if(hsorted.Length == 0) { return; }
            lod2HindicesCPUSide = new List<int>(hsorted.Length / 8);
            lod2HindicesCPUSide.Add(0);
            int prev = hsorted[0];
            int next;
            for(int i=0; i < hsorted.Length; ++i)
            {
                next = hsorted[i];
                if(TestHilbertIndices.isDifferentHilbertCube(prev, next, 2))
                {
                    lod2HindicesCPUSide.Add(i);
                    prev = next;
                }
            }

            Debug.Log("hsorted: " + hsorted.Length);
            Debug.Log("LOD2 indices: " + lod2HindicesCPUSide.Count);
            List<int> lodTwoVoxels = new List<int>(lod2HindicesCPUSide.Count);
            foreach(int index in lod2HindicesCPUSide)
            {
                lodTwoVoxels.Add(hsorted[index]);
            }
            Debug.Log("lodTwoVoxels: " + lodTwoVoxels.Count);
            DebugDrawVoxelOrder.Draw(lodTwoVoxels.ToArray(), vGenConfig.ChunkSizeX + 1);
            //EditorApplication.isPaused = true;
        }

        #endregion


        #region call-kernels

        public void callFaceGenKernel()
        {
            ShownVoxels.SetCounterValue(0);
            meshGen.Dispatch(ExposedVoxelsKernel, vGenConfig.GroupsPerChunkX, vGenConfig.GroupsPerChunkY, vGenConfig.GroupsPerChunkZ); //  8, 1, 8 );

        }

        public void callConstructHilbertIndicesKernel()
        {
            int shownVoxelsCount = getShownVoxelCount();
            if(shownVoxelsCount < 1) { return; }

            releaseHilbert();
            HilbertIndices = new ComputeBuffer(shownVoxelsCount, sizeof(uint), ComputeBufferType.Counter);
            HilbertLODRanges = new ComputeBuffer(vGenConfig.NumLODLevels, sizeof(uint));
            hilbertIndexShader.SetBuffer(ConstructHilbertIndicesKernel, "hIndexRanges", HilbertLODRanges);
            hilbertIndexShader.SetBuffer(ConstructHilbertIndicesKernel, "hilbertIndices", HilbertIndices);
            hilbertIndexShader.SetBuffer(ConstructHilbertIndicesKernel, "ShownVoxels", ShownVoxels);
            HilbertIndices.SetCounterValue(0);
            //
            // hilbertIndexShader.Dispatch(ConstructHilbertIndicesKernel, 1, 1, 1);
            //
            Debug.Log("Declining to call ConstructHilbertIKernel");
        }

        public void callFaceCopyKernel()
        {
            BufferCountArgs args = getBufferCount(ShownVoxels);
            if(args.count < 1) { return; }

            releaseSolidVoxels();

            SolidVoxels = new ComputeBuffer(args.count, sizeof(uint));
            meshGen.SetBuffer(FaceCopyKernel, "SolidVoxels", SolidVoxels);

            //ifdef CPU SIDE

            releaseHilbert();
            HilbertIndices = new ComputeBuffer(lod2HindicesCPUSide.Count, sizeof(int));
            meshGen.SetBuffer(FaceCopyKernel, "hilbertIndices", HilbertIndices);

            releaseOutHilbert();
            OutHilbertIndices = new ComputeBuffer(lod2HindicesCPUSide.Count, sizeof(int));
            meshGen.SetBuffer(FaceCopyKernel, "outHilbertIndices", OutHilbertIndices);

            meshGen.Dispatch(FaceCopyKernel, 1, 1, 1);

            // else

            /* // ifndef CPU SIDE
            meshGen.SetBuffer(FaceCopyKernel, "hilbertIndices", HilbertIndices);

            BufferCountArgs hargs = getBufferCount(HilbertIndices);

            releaseOutHilbert(); //want this here? 
            OutHilbertIndices = new ComputeBuffer(Mathf.Max(1, hargs.count), sizeof(uint));
            meshGen.SetBuffer(FaceCopyKernel, "outHilbertIndices", OutHilbertIndices);

            meshGen.Dispatch(FaceCopyKernel, 1, 1, 1);
            */

            // endif
        }

        #endregion

        void setTemporaryBuffers()
        {
            meshGen.SetBuffer(ExposedVoxelsKernel, "MapVoxels", voxelMapData.MapVoxels);
            meshGen.SetBuffer(ExposedVoxelsKernel, "MapHeights", voxelMapData.MapHeights);
            meshGen.SetBuffer(ExposedVoxelsKernel, "ShownVoxelCount", ShownVoxelCount);
            meshGen.SetBuffer(ExposedVoxelsKernel, "ShownVoxels", ShownVoxels);

            //meshGen.SetBuffer(FaceSumKernel, "ShownVoxelCount", ShownVoxelCount);
            //meshGen.SetBuffer(FaceSumKernel, "ShownVoxelOffset", ShownVoxelOffset);
            //meshGen.SetBuffer(FaceSumKernel, "TotalVoxelCount", TotalVoxelCount);

            meshGen.SetBuffer(FaceCopyKernel, "ShownVoxels", ShownVoxels);
            meshGen.SetBuffer(FaceCopyKernel, "ShownVoxelCount", ShownVoxelCount);
            meshGen.SetBuffer(FaceCopyKernel, "ShownVoxelOffset", ShownVoxelOffset);

            meshGen.SetBuffer(GetFacesKernel, "MapVoxels", voxelMapData.MapVoxels);
            meshGen.SetBuffer(GetFacesKernel, "TotalVoxelCount", TotalVoxelCount);

            //meshGen.SetBuffer(ExposedMip64Kernel, "MapVoxels", voxelMapData.MapVoxels);
            //meshGen.SetBuffer(ExposedMip64Kernel, "MapHeights", voxelMapData.MapHeights);
            //meshGen.SetBuffer(ExposedMip64Kernel, "ShownVoxels", ShownVoxels);
        }


        public Mesh[] getMeshes() {
            return null;
            // NEVER USED??
            //ComputeBuffer SolidFaces = new ComputeBuffer( totalShownVoxelCount, sizeof( int ));

            //  meshGen.SetBuffer( GetFacesKernel, "SolidVoxels", SolidVoxels );
            //  meshGen.SetBuffer( GetFacesKernel, "SolidFaces", SolidFaces );

            //  meshGen.Dispatch( GetFacesKernel, (totalShownVoxelCount+1023)/1024, 1, 1 );

            //int[] voxels = new int[totalShownVoxelCount];

            //  SolidFaces.GetData( voxels );
            //  SolidFaces.Dispose();

            //List<Mesh> meshes = new List<Mesh>();

            //  for (int offset=0; offset<totalShownVoxelCount; offset += 2730)
            //  {
            //    meshes.Add( makeMesh( voxels, offset, offset+2730 <= totalShownVoxelCount ? 2730 : totalShownVoxelCount-offset ));
            //  }

            //  return meshes.ToArray();
        }


        public GameObject getTerrain() {
            return null; //Don't want to use getTerrain

            /*
            int[] heights = new int[256 * 256];

            voxelMapData.MapHeights.GetData(heights);

            float[,] terrainHeights = new float[256, 256];

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                    terrainHeights[x, z] = heights[x * 256 + z] / 256.0f;

            TerrainData terrainData = new TerrainData();

            //      terrainData.size = new Vector3( 256, 256, 256 );
            terrainData.size = new Vector3(128, 1024, 128);
            terrainData.heightmapResolution = 256;
            terrainData.baseMapResolution = 256;
            terrainData.SetDetailResolution(256, 16);

            terrainData.SetHeightsDelayLOD(0, 0, terrainHeights);

            return Terrain.CreateTerrainGameObject(terrainData);
            */
        }


        static Vector3
          v3000 = new Vector3(-0.5f, -0.5f, -0.5f),
          v3001 = new Vector3(-0.5f, -0.5f, 0.5f),
          v3010 = new Vector3(-0.5f, 0.5f, -0.5f),
          v3011 = new Vector3(-0.5f, 0.5f, 0.5f),
          v3100 = new Vector3(0.5f, -0.5f, -0.5f),
          v3101 = new Vector3(0.5f, -0.5f, 0.5f),
          v3110 = new Vector3(0.5f, 0.5f, -0.5f),
          v3111 = new Vector3(0.5f, 0.5f, 0.5f);

        Mesh makeMesh(int[] vox, int start, int count) {
            Vector3[] vertices = new Vector3[count * 4 * 6];
            int vertexCount = 0;

            for (int i = 0; i < count; i++) {
                int v = vox[start + i];
                Vector3 pos = new Vector3((v >> 16) & 255, (v >> 8) & 255, v & 255);
                int facing = (v >> 24) & 255;

                if (0 != (facing & 0x1)) {
                    vertices[vertexCount++] = v3001 + pos;
                    vertices[vertexCount++] = v3011 + pos;
                    vertices[vertexCount++] = v3000 + pos;
                    vertices[vertexCount++] = v3010 + pos;
                }

                if (0 != (facing & 0x2)) {
                    vertices[vertexCount++] = v3100 + pos;
                    vertices[vertexCount++] = v3110 + pos;
                    vertices[vertexCount++] = v3101 + pos;
                    vertices[vertexCount++] = v3111 + pos;
                }

                if (0 != (facing & 0x4)) {
                    vertices[vertexCount++] = v3001 + pos;
                    vertices[vertexCount++] = v3000 + pos;
                    vertices[vertexCount++] = v3101 + pos;
                    vertices[vertexCount++] = v3100 + pos;
                }

                if (0 != (facing & 0x8)) {
                    vertices[vertexCount++] = v3010 + pos;
                    vertices[vertexCount++] = v3011 + pos;
                    vertices[vertexCount++] = v3110 + pos;
                    vertices[vertexCount++] = v3111 + pos;
                }

                if (0 != (facing & 0x10)) {
                    vertices[vertexCount++] = v3000 + pos;
                    vertices[vertexCount++] = v3010 + pos;
                    vertices[vertexCount++] = v3100 + pos;
                    vertices[vertexCount++] = v3110 + pos;
                }

                if (0 != (facing & 0x20)) {
                    vertices[vertexCount++] = v3101 + pos;
                    vertices[vertexCount++] = v3111 + pos;
                    vertices[vertexCount++] = v3001 + pos;
                    vertices[vertexCount++] = v3011 + pos;
                }
            }

            Array.Resize<Vector3>(ref vertices, vertexCount);

            int[] triangles = new int[vertexCount / 4 * 6];

            for (int i = 0, j = 0; i < vertexCount; i += 4, j += 6) {
                triangles[j] = i;
                triangles[j + 1] = i + 1;
                triangles[j + 2] = i + 2;
                triangles[j + 3] = i + 2;
                triangles[j + 4] = i + 1;
                triangles[j + 5] = i + 3;
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            ;

            return mesh;
        }


        public void releaseTemporaryBuffers() {
            ComputeBuffer[] buffs = new ComputeBuffer[]
            {
                ShownVoxelCount,
                ShownVoxelOffset,
                ShownVoxels,
                argBuffer,
                TotalVoxelCount,
                HilbertIndices,
                OutHilbertIndices,
                HilbertLODRanges,
            };
            int i = 0;
            foreach(var buff in buffs)
            {
                if(buff == null)
                {
                    Debug.Log("buff " + i + "was already null");
                }
                i++;
                releaseBuffer(buff);
            }
           
        }

        void releaseBuffer(ComputeBuffer buff)
        {
            if(buff != null)
            {
                buff.Release();
            }
            buff = null;
        }
    }
}

