using UnityEngine;
using System.Collections;
using Mel.Math;

namespace Mel.VoxelGen
{
    //TODO: modularizable, chainable, combinable noise generators
    // Class NModule. Has an array of inputs, etc..

    public class CVoxelMapData
    {
        public ComputeBuffer MapVoxels { get; private set; }
        public ComputeBuffer MapVoxelsLOD2 { get; private set; }
        public ComputeBuffer MapVoxelsLOD4 { get; private set; }

        ComputeShader perlinGen;
        private VGenConfig vGenConfig;
        int PerlinMapGenKernel;

        int ClearMapBuffersKernel;

        public CVoxelMapData(ComputeShader shader, VGenConfig vGenConfig)
        {
            perlinGen = shader;
            this.vGenConfig = vGenConfig;

            initKernels();
            initTemporaryBuffers();
        }

        void initKernels()
        {
            PerlinMapGenKernel = perlinGen.FindKernel("PerlinMapGen");
            ClearMapBuffersKernel = perlinGen.FindKernel("ClearMapBuffers");
        }

        void initTemporaryBuffers()
        {
            MapVoxels = new ComputeBuffer(vGenConfig.ChunkPerlinGenArraySize, sizeof(int));
            MapVoxelsLOD2 = new ComputeBuffer(vGenConfig.VoxelsPerChunk / 8 /* - V/64 */, sizeof(int));
            MapVoxelsLOD4 = new ComputeBuffer(vGenConfig.VoxelsPerChunk / 64, sizeof(int));

            perlinGen.SetBuffer(PerlinMapGenKernel, "MapVoxels", MapVoxels);
            perlinGen.SetBuffer(PerlinMapGenKernel, "MapVoxelsLOD2", MapVoxelsLOD2);
            perlinGen.SetBuffer(PerlinMapGenKernel, "MapVoxelsLOD4", MapVoxelsLOD4);

            perlinGen.SetBuffer(ClearMapBuffersKernel, "MapVoxels", MapVoxels);
            perlinGen.SetBuffer(ClearMapBuffersKernel, "MapVoxelsLOD2", MapVoxelsLOD2);
            perlinGen.SetBuffer(ClearMapBuffersKernel, "MapVoxelsLOD4", MapVoxelsLOD4);

        }

        public void releaseTemporaryBuffers()
        {
            if (null != MapVoxels)
                MapVoxels.Release();
            MapVoxels = null;

            if (null != MapVoxelsLOD2)
                MapVoxelsLOD2.Release();
            MapVoxelsLOD2 = null;

            if (MapVoxelsLOD4 != null)
                MapVoxelsLOD4.Release();
            MapVoxelsLOD4 = null;
        }

        public void setMapOffset(IntVector3 mapoffset)
        {
            perlinGen.SetVector("MapOffset", (Vector3)vGenConfig.ChunkPosToPos(mapoffset));
        }


        public void callPerlinMapGenKernel(IntVector3 chunkPos)
        {
            setMapOffset(chunkPos);
            callPerlinMapGenKernel();
        }

        void callPerlinMapGenKernel()
        {
            perlinGen.Dispatch(PerlinMapGenKernel, vGenConfig.GroupsPerChunkX, vGenConfig.GroupsPerChunkY, vGenConfig.GroupsPerChunkZ);
        }

        public void callClearMapBuffersKernel()
        {
            var lodGroups = vGenConfig.GroupsPerChunkAtLOD(0);
            perlinGen.Dispatch(ClearMapBuffersKernel,  lodGroups.x, lodGroups.y, lodGroups.z);
        }

        public uint[] TestGetMapVoxelData()
        {
            uint[] mvs = new uint[MapVoxels.count];
            MapVoxels.GetData(mvs);
            return mvs;
        }

        public void DebugBuffs()
        {
            //DebugChunkData.CheckUniqueData(MapVoxels, "MapVoxels");
            //DebugChunkData.CheckUniqueData(MapVoxelsLOD2, "MapVoxLOD2");
            //DebugChunkData.CheckUniqueData(MapVoxelsLOD4, "MapVoxLOD4");

            //DebugChunkData.CheckAllSamePositionOnAxis(MapVoxelsLOD4, 0, "MapVoxLOD4");

        }

    }
}
