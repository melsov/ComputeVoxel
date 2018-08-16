using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mel.Editorr;
using Mel.Math;
#if UNITY_EDITOR
using UnityEditor;


#endif



namespace Mel.VoxelGen
{

#if UNITY_EDITOR
    [CustomEditor(typeof(VGenConfig))]
    public class VGenConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Update ChunkConstants.cginc"))
            {
                ((VGenConfig)target).runUpdate();
            }
            if(GUILayout.Button("Update Hilbert Tables"))
            {
                ((VGenConfig)target).updateHilbertTables();
            }
        }
    }
#endif

    public struct VoxelGeomDataMirror
    {
        public uint voxel;
    }

    public struct VoxelGenDataMirror
    {
        public uint voxel;
    }

    public class VGenConfig : MonoBehaviour {

        [SerializeField]
        private int _ChunkDimension = 32;

        public int ChunkDimension { get { return _ChunkDimension; } }

        //assuming cubic chunks...
        //public int ChunkDimLog2 {
        //    get {
        //        int i = 1;
        //        for (int n = 2; n < _ChunkDimension; n *= 2) i++;
        //        return i;
        //    }
        //}

        public int ChunkSizeX { get { return _ChunkDimension; } }
        public int ChunkSizeY { get { return _ChunkDimension; } }
        public int ChunkSizeZ { get { return _ChunkDimension; } }

        public IntVector3 ChunkSize {
            get {
                return new IntVector3() { x = ChunkSizeX, y = ChunkSizeY, z = ChunkSizeZ };
            }
        }

        public IntVector3 ChunkPosToPos(IntVector3 chunkPos)
        {
            return chunkPos * ChunkSize;
        }

        public Bounds ChunkPosToBounds(IntVector3 chunkPos)
        {
            return new Bounds(ChunkPosToPos(chunkPos) + ChunkSize / 2, ChunkSize);
        }

        public IntVector3 PosToChunkPos(IntVector3 pos)
        {
            return pos / ChunkSize;
        }

        [SerializeField, Header("World height. Will clip value % ChunkDimension")] int _BedRockToMax = 256;
        public int MaxHeightInChunks {
            get { return _BedRockToMax / ChunkSizeY; }
        }
        public int BedRockToMax { get { return MaxHeightInChunks * ChunkSizeY; } }
        public int SeaLevel { get { return BedRockToMax / 4; } }

        [Header("ATM, ChunkThreadsPerGroup X,Y,Z should all be the same number. (Shaders sometimes use them interchangeably.)")]
        [Header("Max allowed threads per group (X * Y * Z) is 1024 (in Shader Model 5.0)")]
        public int ChunkThreadsPerGroupX = 8;
        public int ChunkThreadsPerGroupY = 8;
        public int ChunkThreadsPerGroupZ = 8;



        public readonly int VoxelsPerMapData = 4; 
        public int MapDataDimY { get { return Mathf.Max(1, ChunkSizeY / VoxelsPerMapData); } }
        public int VoxelsPerKernelYColumn { get { return ChunkSizeY; } }


        public int ColumnsPerChunk { get { return ChunkSizeX * ChunkSizeZ; } }

        public int VoxelsPerChunk { get { return ChunkSizeX * ChunkSizeY * ChunkSizeZ; } }

        internal int VoxelsPerChunkAtLOD(int octantDepth)
        {
            return VoxelsPerChunk / (int)Mathf.Pow(8, octantDepth);
        }
        public int ChunkPerlinGenArraySize { get { return VoxelsPerChunk / VoxelsPerMapData; } }


        public int GroupsPerChunkX { get { return ChunkSizeX / ChunkThreadsPerGroupX; } }
        public int GroupsPerChunkY { get { return ChunkSizeY / ChunkThreadsPerGroupY; } }
        public int GroupsPerChunkZ { get { return ChunkSizeZ / ChunkThreadsPerGroupZ; } }

        

        public IntVector3 GroupsPerChunk { get { return new IntVector3(GroupsPerChunkX, GroupsPerChunkY, GroupsPerChunkZ); } }

        //
        // The naming convention used throughout is LOD2 for 2x2x2 voxel LOD, LOD4 for 4x4x4
        // lodLog2 = 1 for LOD2, = 2 for LOD4
        //
        public IntVector3 GroupsPerChunkAtLOD(int lodLog2)
        {
            return GroupsPerChunk / Mathf.Pow(2f, lodLog2);
        }

        //
        // In the noise gen kernel, voxels are packed 4 per int (VoxelsPerMapData)
        // Y is mapped as the least significant dimension in the flat 'MapVoxels' buffer.
        //
        public int ChunkNoiseThreadsPerGroupY {
            get {
                return ChunkThreadsPerGroupY / VoxelsPerMapData; // please crash if zero
            }
        }

        //
        // TODO: purge references to MIP64
        //
//        [SerializeField]
        private int _Mip64Divisor = 4;
        public int Mip64Divisor { get { return _Mip64Divisor; } }

        public int ChunkMip64ThreadsPerGroupX { get { return ChunkThreadsPerGroupX / _Mip64Divisor; } }
        public int ChunkMip64ThreadsPerGroupY { get { return ChunkThreadsPerGroupY / _Mip64Divisor; } }
        public int ChunkMip64ThreadsPerGroupZ { get { return ChunkThreadsPerGroupZ / _Mip64Divisor; } }

        int MipVoxelsPerChunk { get { return VoxelsPerChunk / (_Mip64Divisor * _Mip64Divisor * _Mip64Divisor); } }

        int _hilbertBits = -1;
        public int hilbertBits {
            get {
                if(_hilbertBits < 0)
                {
                    _hilbertBits = GetHilbertBits(VoxelsPerChunk);
                    print("Hilbert bits: " + _hilbertBits);
                }
                return _hilbertBits;
            }
        }

        public static int GetHilbertBits(int CubeArea)
        {
            int result = -1;
            while (Mathf.Pow(8, ++result) < CubeArea) ;
            return result;
        }

        public readonly int NumLODLevels = 3;
        [SerializeField]
        AnimationCurve LODDistribution;
        [SerializeField]
        float fullLODRadiusSquared = 1024f;

        public int LODIndexForCamDistance(float camDistance)
        {
            float normalizedDistance = camDistance / (fullLODRadiusSquared * NumLODLevels);
            return Mathf.FloorToInt(Mathf.Clamp( LODDistribution.Evaluate(Mathf.Clamp01(normalizedDistance)) * NumLODLevels, 0f, NumLODLevels - .01f));
        }



        public Vector3 NoiseInputDivisor = Vector3.one * 150f;

        //public int NoiseGroupsPerChunkY {
        //    get {
        //        return ChunkSizeY / ChunkThreadsPerGroupY;
        //    }
        //}

        public int MipLevels = 5;


        [SerializeField, Header("Debug")]
        public bool TestAlwaysAVoxel;
        public bool TestShowAll;
        [SerializeField]
        public bool TestFillChunkEdges;

        public enum TestShape
        {
            None, Ramp, OneInACorner, WallX0, Sphere2C, Empty,
        }
        [SerializeField] TestShape testShape;
        public string TestShapeDirective {
            get {
                string start = "#define TEST_SHAPE_";
                switch(testShape)
                {
                    case TestShape.None:
                    default:
                        return start + "NONE";
                    case TestShape.Ramp:
                        return start + "RAMP";
                    case TestShape.OneInACorner:
                        return start + "ONE_IN_CORNER";
                    case TestShape.WallX0:
                        return start + "WALLX0";
                    case TestShape.Sphere2C:
                        return start + "SPHERE2C";
                    case TestShape.Empty:
                        return start + "EMPTY";
                }
            }
        }

        public enum TestChunkSelect
        {
            All, EvenCoords
        }
        [SerializeField] TestChunkSelect testChunkSelect;
        public string TestChunkSelectDirective {
            get {
                string start = "#define TEST_CHUNK_SELECT_";
                switch (testChunkSelect)
                {
                    case TestChunkSelect.All:
                    default:
                        return start + "ALL";
                    case TestChunkSelect.EvenCoords:
                        return start + "EVEN_COORDS";
                }
            }
        }

        public bool IsSerializedDataValid {
            get {
                return PlayerPrefs.GetInt("SerDataValid") == 1;
            }
            set {
                PlayerPrefs.SetInt("SerDataValid", value ? 1 : 0);
            }
        }

        [SerializeField] bool DebugDontComputeChunksIfSerDataValid = true;

        public bool DebugDontComputeChunks {
            get { return DebugDontComputeChunksIfSerDataValid && IsSerializedDataValid; }
        }


        public struct RevCastDataMirror
        {
            public int x, y, z, w; //int4
        }

        [SerializeField] int _ReverseCastThreadCount = 8;
        public int ReverseCastThreadCount { get { return _ReverseCastThreadCount; } }

        public IntVector2 ReverseCastClearKernelThreadCount { get {
                return new IntVector2
                {
                    x = 64,
                    y = 1
                };
            }
        }

        public IntVector2 ReverseCastClearKernelThreadGroups {
            get {
                return new IntVector2 {
                    x = ReverseCastBufferResolutionXY.x / ReverseCastClearKernelThreadCount.x,
                    y = ReverseCastBufferResolutionXY.y / ReverseCastClearKernelThreadCount.y };
            }
        }

        [SerializeField] IntVector2 _ReverseCastBufferResolutionXY;
        public IntVector2 ReverseCastBufferResolutionXY { get { return _ReverseCastBufferResolutionXY; } }
        public int ReverseCastBufferSize { get { return ReverseCastBufferResolutionXY.Area; } }
        public bool UseReverseCasting = true;

        public void runUpdate()
        {
            IsSerializedDataValid = false;
            WriteChunkConstantsCGInc.WriteChunkCGInc();
        }

        [SerializeField]
        HilbertTableWriter hilbertTableWriter;

        internal void updateHilbertTables()
        {
            hilbertTableWriter.Write( (uint)ChunkDimension);
        }
    }
}
