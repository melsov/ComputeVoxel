#ifndef VOXEL_HELPER
#define VOXEL_HELPER

#include "ChunkConstants.cginc"

uint encodeVoxelPosition(uint voxelid, int x, int y, int z) {
	return ((voxelid * 256 + x) * 256 + y) * 256 + z;
}

uint encodeVoxelPosition(uint voxelid, uint3 p) {
	return ((voxelid * 256 + p.x) * 256 + p.y) * 256 + p.z;
}

uint3 decodeVoxelPosition(uint voxel) {
	return uint3( voxel/65536 % CHUNK_DIM_X, voxel/256 % CHUNK_DIM_Y, voxel % CHUNK_DIM_Z);
}

uint decodeVoxelID(uint voxel){
	return (voxel >> 24) & 255;
}

bool isDifferentCube(uint3 p, uint3 n, uint cubeSize) {
	p /= cubeSize;
	n /= cubeSize;

	return p.x != n.x || p.y != n.y || p.z != n.z;
}

float MinAbs(float3 f) {
	f = abs(f);
	if(f.x < f.y) {
		return (f.x < f.z) ? f.x : f.z;
	} else {
		return (f.y < f.z) ? f.y : f.z;
	}
}

float LengthSquared(float3 f) { return f.x * f.x + f.y * f.y + f.z * f.z;}

#endif
