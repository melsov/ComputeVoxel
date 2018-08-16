#ifndef CHUNK_CONSTANTS
#define CHUNK_CONSTANTS

// Values must match VGenConfig 
// CONSIDER: editor script to set these

#define CHUNK_DIM_X 256
#define CHUNK_DIM_Y 256
#define CHUNK_DIM_Z 256

#define COLUMNS_PER_CHUNK (CHUNK_DIM_X * CHUNK_DIM_Z)
#define VOXELS_PER_CHUNK (CHUNK_DIM_X * CHUNK_DIM_Y * CHUNK_DIM_Z)

#define CHUNK_THREADS_X 32
#define CHUNK_THREADS_Y 1
#define CHUNK_THREADS_Z 32

#define VOXELS_PER_MAP_DATA 4
#define MAP_DATA_DIM_Y (CHUNK_DIM_Y / VOXELS_PER_MAP_DATA)

#endif
