#ifndef CHUNK_CONSTANTS
#define CHUNK_CONSTANTS

// Values copied from VGenConfig
// Use editor menu: MEL / Write Chunk CGInc to change them


#define CHUNK_DIM_X 64
#define CHUNK_DIM_Y 64
#define CHUNK_DIM_Z 64

#define COLUMNS_PER_CHUNK 4096
#define VOXELS_PER_CHUNK 262144

#define CHUNK_THREADS_X 8
#define CHUNK_THREADS_Y 8
#define CHUNK_THREADS_Z 8

#define VOXELS_PER_MAP_DATA 4
#define MAP_DATA_DIM_Y  16

#define BEDROCK_TO_MAX 256
#define SEA_LEVEL 64


#define INTERLEAVE_DISPLAY_COLUMNS

#define MIP_LEVELS uint(2)

#define CHUNK_NOISE_THREADS_Y uint(2)

#define CHUNK_MIP64_THREADS_X 2
#define CHUNK_MIP64_THREADS_Y 2
#define CHUNK_MIP64_THREADS_Z 2

#define MIP_64_DIVISOR 4

#define NOISE_INPUT_DIVISOR float3(1000,1000,1000)


#define TEX_TILE_SCALE 4
//Test
//#define TEST_SHOW_ALL
//#define TEST_ALWAYS_A_VOXEL
#define TEST_SHAPE_NONE // Test shape?
#define TEST_CHUNK_SELECT_ALL // Test chunk select?

#define EmptyVoxel 0

struct GeomVoxelData
{
   uint voxel;
};

struct RevCastData
{
   int4 voxel;
};

#define REVERSE_CAST_THREAD_COUNT 8

#define BUFF_THREADS_X 64
#define BUFF_THREADS_Y 1

//#define FILL_CHUNK_EDGES


#endif
