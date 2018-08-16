using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Mel.VoxelGen;

#if UNITY_EDITOR
using UnityEditor;

namespace Mel.Editorr
{
    public class WriteChunkConstantsCGInc : MonoBehaviour
    {

        static string filePath = "VoxelPerformance/Shaders/ChunkConstants.cginc";
        static string path {
            get { return Application.dataPath + "/" + filePath;  }
        }

        static string ConfigFormat = @"#ifndef CHUNK_CONSTANTS
#define CHUNK_CONSTANTS

// Values copied from VGenConfig
// Use editor menu: MEL / Write Chunk CGInc to change them


#define CHUNK_DIM_X {0}
#define CHUNK_DIM_Y {1}
#define CHUNK_DIM_Z {2}

#define COLUMNS_PER_CHUNK {3}
#define VOXELS_PER_CHUNK {4}

#define CHUNK_THREADS_X {5}
#define CHUNK_THREADS_Y {6}
#define CHUNK_THREADS_Z {7}

#define VOXELS_PER_MAP_DATA {8}
#define MAP_DATA_DIM_Y  {9}

#define BEDROCK_TO_MAX {10}
#define SEA_LEVEL {11}


#define INTERLEAVE_DISPLAY_COLUMNS

#define MIP_LEVELS uint({12})

#define CHUNK_NOISE_THREADS_Y uint({13})

#define CHUNK_MIP64_THREADS_X {14}
#define CHUNK_MIP64_THREADS_Y {15}
#define CHUNK_MIP64_THREADS_Z {16}

#define MIP_64_DIVISOR {17}

#define NOISE_INPUT_DIVISOR float3({18},{19},{20})


#define TEX_TILE_SCALE 4
//Test
{21}#define TEST_SHOW_ALL
{22}#define TEST_ALWAYS_A_VOXEL
{23} // Test shape?
{24} // Test chunk select?

#define EmptyVoxel 0

struct GeomVoxelData
{{
   uint voxel;
}};

struct RevCastData
{{
   int4 voxel;
}};

#define REVERSE_CAST_THREAD_COUNT {25}

#define BUFF_THREADS_X {26}
#define BUFF_THREADS_Y {27}

{28}#define FILL_CHUNK_EDGES


#endif
";

        [MenuItem("MEL/Write Chunk CGInc %&a")]
        public static void WriteChunkCGInc() {
            VGenConfig vGenConfig = FindObjectOfType<VGenConfig>();

            string config = string.Format(ConfigFormat,
                vGenConfig.ChunkSizeX,
                vGenConfig.ChunkSizeY,
                vGenConfig.ChunkSizeZ,
                vGenConfig.ColumnsPerChunk,
                vGenConfig.VoxelsPerChunk,
                vGenConfig.ChunkThreadsPerGroupX,
                vGenConfig.ChunkThreadsPerGroupY,
                vGenConfig.ChunkThreadsPerGroupZ,
                vGenConfig.VoxelsPerMapData,
                vGenConfig.MapDataDimY,
                vGenConfig.BedRockToMax, 
                vGenConfig.SeaLevel, 
                vGenConfig.MipLevels,
                vGenConfig.ChunkNoiseThreadsPerGroupY, 
                vGenConfig.ChunkMip64ThreadsPerGroupX,
                vGenConfig.ChunkMip64ThreadsPerGroupY,
                vGenConfig.ChunkMip64ThreadsPerGroupZ,
                vGenConfig.Mip64Divisor,
                vGenConfig.NoiseInputDivisor.x, vGenConfig.NoiseInputDivisor.y, vGenConfig.NoiseInputDivisor.z,
                vGenConfig.TestShowAll ? String.Empty : "//",
                vGenConfig.TestAlwaysAVoxel ? String.Empty : "//",
                vGenConfig.TestShapeDirective,
                vGenConfig.TestChunkSelectDirective,
                vGenConfig.ReverseCastThreadCount, 
                vGenConfig.ReverseCastClearKernelThreadCount.x,
                vGenConfig.ReverseCastClearKernelThreadCount.y,
                vGenConfig.TestFillChunkEdges ? String.Empty : "//"

                );
            File.WriteAllText(path, config);
            AssetDatabase.Refresh();
        }

        [MenuItem("MEL/Alternate Play Button %&c")]
        static void AlternatePlayButton() {
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    }
}

#else

#endif
