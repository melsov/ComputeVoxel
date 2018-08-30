#ifndef VOXEL_GEOM_HELPER
#define VOXEL_GEOM_HELPER

#include "UnityCG.cginc"
#include "ChunkConstants.cginc"

//
// Geom shader to frag
//
struct input
{
	float4 pos : SV_POSITION;
	float4 _color : COLOR;
	float2 uv : TEXCOORD0;
};

//
// Vert to geom shader
//
struct inputGS
{
	float4 pos : SV_POSITION;
	float4 _color : COLOR;
	float4 uvOffset : TEXCOORD0;
	uint4 extras : TEXCOORD1;
};

static const float4 NegativeFaces[12] =
{
//
// X negative faces
//
	float4( -1.0, -1.0, 1.0, 0 ),
	float4( -1.0, 1.0, 1.0, 0 ),
	float4( -1.0, -1.0, -1.0, 0 ),
	float4( -1.0, 1.0, -1.0, 0 ), 

//
// Y negative faces
//
	float4( -1.0, -1.0, 1.0, 0 ),
	float4( -1.0, -1.0, -1.0, 0 ),
	float4( 1.0, -1.0, 1.0, 0 ),
	float4( 1.0, -1.0, -1.0, 0 ), 
  
//
// Z negative faces
//
	float4( -1.0, -1.0, -1.0, 0 ),
	float4( -1.0, 1.0, -1.0, 0 ),
	float4( 1.0, -1.0, -1.0, 0 ),
	float4( 1.0, 1.0, -1.0, 0 ),
};

#define FACES_OFFSET_X 0
#define FACES_OFFSET_Y 4
#define FACES_OFFSET_Z 8

//
// axis enum
//
#define AXIS_X 0
#define AXIS_Y 1
#define AXIS_Z 2

/*
Neighbor12 bit order reverse lookup
(0, -1, -1),(-1, -1, 0),(1, -1, 0),(0, -1, 1),(-1, 0, -1),(1, 0, -1),(-1, 0, 1),(1, 0, 1),(0, 1, -1),(-1, 1, 0),(1, 1, 0),(0, 1, 1),
*/

static const float3 Neighbor12BitToPos[12] =
{
	float3(0, -1, -1),float3(-1, -1, 0),float3(1, -1, 0),float3(0, -1, 1),float3(-1, 0, -1),float3(1, 0, -1),float3(-1, 0, 1),float3(1, 0, 1),float3(0, 1, -1),float3(-1, 1, 0),float3(1, 1, 0),float3(0, 1, 1)
};



struct LocalShadowNeighbors
{
	int3 a;
	int3 b;
	int3 corner;
};

//
// Excpects v to point to a cube vertex 
// from center with ones and negative ones
//
LocalShadowNeighbors GetLocalShadowNeighbors(int3 v, int axis)
{
	LocalShadowNeighbors neibs;

	if(axis == AXIS_X)
	{
		neibs.a = int3(v.x, v.y, 0);
		neibs.b = int3(v.x, 0, v.z);
		// neibs.corner = int3(v.x, 0, v.z);
	}
	else if(axis == AXIS_Y)
	{
		neibs.a = int3(v.x, v.y, 0); 
		neibs.b = int3(0, v.y, v.z);
		// neibs.corner = int3(v.x, v.y, 0);
	}
	else 
	{
		neibs.a = int3(0, v.y, v.z);
		neibs.b = int3(v.x, 0, v.z);
	}
	neibs.corner = int3(v.x, v.y, v.z);

	return neibs;
}

uint Shift(int3 neighborIndex)
{
	neighborIndex += 1;
	return neighborIndex.x * 9 + neighborIndex.y * 3 + neighborIndex.z;

	// uint3 v = neighborIndex + 1;
 //    uint result = v.y << 2;
 //    if (v.y == 1) // middle
 //    {
 //        result |= v.z + (v.x >> 1);
 //    } else
 //    {
 //        result |= (v.z << 1) + v.x - ((v.x * v.z) >> 1) - 1;
 //    }
 //    return result;
}

bool isInShadow(float3 local, int axis, uint neighborBits12)
{
	LocalShadowNeighbors neibs = GetLocalShadowNeighbors(local, axis);
	uint shiftA = Shift(neibs.a);
	uint shiftB = Shift(neibs.b);
	uint shiftCorner = Shift(neibs.corner);
	return 
		((neighborBits12 >> shiftA) & 1) 
		 |((neighborBits12 >> shiftB) & 1)  
	 	|((neighborBits12 >> shiftCorner) & 1)
	 	;
}


input GetVertData(
	input pIn,
	matrix _worldMatrixTransform, 
	float4 pos, 
	uint4 extras,
	float4 shift,
	float cubeDimension,
	int facesOffset,
	int faceCornerIndex)
{
	float4 local = shift *  NegativeFaces[facesOffset + faceCornerIndex];
	
	//
    // Neighbors
    //
    uint neiBits12 = extras.w; 
    bool shadow = isInShadow(local, facesOffset / 4, neiBits12);


    if(shadow){
    	pIn._color *= .65; //more severe shadows when nei bits are meaningful
    }


	pIn.pos = mul( UNITY_MATRIX_VP, mul( _worldMatrixTransform, pos + local * cubeDimension ) ); 

	return pIn;
}


#endif
