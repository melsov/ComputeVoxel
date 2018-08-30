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
	return uint3( (voxel/65536) % CHUNK_DIM_X, (voxel/256) % CHUNK_DIM_Y, voxel % CHUNK_DIM_Z);
}

uint decodeVoxelID(uint voxel){
	return (voxel >> 24) & 255;
}

uint FlatIndexXYZ(uint3 p, uint3 size) 
{
	return ((p.x * size.y) + p.y) * size.z + p.z;
}

uint FlatIndexZXY(uint3 p, uint3 size) 
{
	return ((p.z * size.x) + p.x) * size.y + p.y;
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


uint ExistsIndex(uint x, uint y, uint z){
	return (((z * CHUNK_DIM_X) + x) * CHUNK_DIM_Y + y) / SIZE_OF_INT;
}

uint ExistsIndex(uint3 p){
	return ExistsIndex(p.x, p.y, p.z);
}



bool GetExists(StructuredBuffer<uint> existMap, uint x, uint y, uint z) {
	uint val = existMap[ExistsIndex(x,y,z)];
	return (val >> (y % SIZE_OF_INT)) & 1;
} 

bool GetExists(StructuredBuffer<uint> existMap, uint3 P) {
	return GetExists(existMap, P.x, P.y, P.z);
}

void SetExists(RWStructuredBuffer<uint> existMap, uint x, uint y, uint z, bool exists)
{

	//existMap[ExistsIndex(x,y,z)] = 4294967295; //TEST
	uint index = ExistsIndex(x,y,z);
	uint orig;
	if(exists) {
		InterlockedOr(existMap[index], 1 << (y % SIZE_OF_INT), orig);
	} else {
		InterlockedAnd(existMap[index], ~(1 << (y % SIZE_OF_INT)), orig);
	}
	// existMap[ExistsIndex(x,y,z)] |= (uint)(1 << (y % SIZE_OF_INT));
	// uint p = pow(2, y % SIZE_OF_INT);
	// uint current = existMap[index] / p;
	// current =  min(current, 1);
	// if(exists) 
	// {
	// 	existMap[index] = existMap[index] + p; // * (1 - current);
	// } else 
	// {
	// 	existMap[index] = existMap[index] - p; // * current;
	// }

	//WANT 
	// if(exists){
	// 	existMap[ExistsIndex(x,y,z)] |= 1 << (y % SIZE_OF_INT); 
	// } else {
	// 	existMap[ExistsIndex(x,y,z)] &= ~(1 << (y % SIZE_OF_INT));
	// }
}

void SetExists(RWStructuredBuffer<uint> existMap, uint3 P, bool exists) {
	SetExists(existMap, P.x, P.y, P.z, exists);
}


bool TestPosIsLocal(uint3 p)
{
	return 
		p.x >= 0 && p.x < CHUNK_DIM_X &&
		p.y >= 0 && p.y < CHUNK_DIM_Y &&
		p.z >= 0 && p.z < CHUNK_DIM_Z;

}

//
// 27
//
bool ExistsAt27(StructuredBuffer<uint> ExistsMap27, int3 ipos)
{
	uint3 pos = ipos + CHUNK_SIZE;
	uint3 rel = pos % CHUNK_SIZE;
	uint3 offset3 = pos / CHUNK_SIZE;
	uint index = FlatIndexZXY(offset3, uint3(3,3,3));
	index *= SIZE_OF_EXISTSMAP;
	index += ExistsIndex(rel);
	uint val = ExistsMap27[index];
	return (val >> (rel.y % SIZE_OF_INT)) & 1 == 1;
}

bool ExistsAt27TEST_ASSUME_WITHIN(StructuredBuffer<uint> ExistsMap27, int3 ipos)
{
	uint3 pos = ipos; // + CHUNK_SIZE;
	uint3 rel = pos; // pos % CHUNK_SIZE;
	// uint3 offset3 = pos / CHUNK_SIZE;
	uint index = 13; // FlatIndexZXY(offset3, uint3(3,3,3));
	index *= SIZE_OF_EXISTSMAP;
	uint relIndex = ExistsIndex(rel);
	uint flat = FlatIndexZXY(rel, CHUNK_SIZE);
	uint val = ExistsMap27[index + relIndex];
	return ((val >> (flat % SIZE_OF_INT)) & 1) == 1;
	// return (val >> (rel.y % SIZE_OF_INT)) & 1 == 1;
}


bool ExistsAt27(StructuredBuffer<uint> ExistsMap27, int x, int y, int z) 
{
	return ExistsAt27(ExistsMap27, int3(x,y,z));
}

/*
Neighbor12 bit order reverse lookup
(0, -1, -1),(-1, -1, 0),(1, -1, 0),(0, -1, 1),(-1, 0, -1),(1, 0, -1),(-1, 0, 1),(1, 0, 1),(0, 1, -1),(-1, 1, 0),(1, 1, 0),(0, 1, 1),
*/

uint SetBit(uint storage, uint position, bool exists)
{
	if(exists){
		return storage | (1 << position);
	} else {
		return storage & ~(1 << position);
	}
}

static const float3 Neighbor12BitToPos[12] =
{
	float3(0, -1, -1),float3(-1, -1, 0),float3(1, -1, 0),float3(0, -1, 1),float3(-1, 0, -1),float3(1, 0, -1),float3(-1, 0, 1),float3(1, 0, 1),float3(0, 1, -1),float3(-1, 1, 0),float3(1, 1, 0),float3(0, 1, 1)
};

static const int3 Cube27XYZRelative[27] = 
{
	int3(-1, -1, -1), int3(-1, -1, 0), int3(-1, -1, 1), 
	int3(-1, 0, -1), int3(-1, 0, 0), int3(-1, 0, 1), 
	int3(-1, 1, -1), int3(-1, 1, 0), int3(-1, 1, 1), 

	int3(0, -1, -1), int3(0, -1, 0), int3(0, -1, 1), 
	int3(0, 0, -1), int3(0, 0, 0), int3(0, 0, 1), 
	int3(0, 1, -1), int3(0, 1, 0), int3(0, 1, 1), 

	int3(1, -1, -1), int3(1, -1, 0), int3(1, -1, 1), 
	int3(1, 0, -1), int3(1, 0, 0), int3(1, 0, 1), 
	int3(1, 1, -1), int3(1, 1, 0), int3(1, 1, 1) 
};

static const int3 RelativeCenterXYZ[27] = 
{
	int3(-1, -1, -1), int3(-1, -1, 0), int3(-1, -1, 1), 
	int3(-1, 0, -1), int3(-1, 0, 0), int3(-1, 0, 1), 
	int3(-1, 1, -1), int3(-1, 1, 0), int3(-1, 1, 1), 
	int3(0, -1, -1), int3(0, -1, 0), int3(0, -1, 1), 
	int3(0, 0, -1), int3(0, 0, 0), int3(0, 0, 1), 
	int3(0, 1, -1), int3(0, 1, 0), int3(0, 1, 1), 
	int3(1, -1, -1), int3(1, -1, 0), int3(1, -1, 1), 
	int3(1, 0, -1), int3(1, 0, 0), int3(1, 0, 1), 
	int3(1, 1, -1), int3(1, 1, 0), int3(1, 1, 1), 
};

static const int3 Neighbors6[6] = 
{
	int3(1, 0, 0),
	int3(-1, 0, 0), 
	int3(0, 1, 0), 
	int3(0, -1, 0), 
	int3(0, 0, 1), 
	int3(0, 0, -1), 
};



#endif
