#ifndef TERRAIN_GENERATION
#define TERRAIN_GENERATION

#include "ChunkConstants.cginc"
#include "3dNoise/noiseSimplex.cginc"


#define GRADIENT_HEIGHT 256
#define SOLID_TERRAIN_THRESHHOLD .6

#define INPUT_SCALE (float3(.015, .015, .015))
#define INPUT_OFFSET (float3(  0,   0,  0))  

#define INPUT_SCALE_2D (float2(.005, .003))
#define INPUT_OFFSET_2D (float2(  0,  0))


float gradient(float samplePoint, float gradientHeight) 
{
    return 1 - saturate(samplePoint / gradientHeight); // 'saturate' is HLSL for 'clamp between zero and 1'
}

int threshhold(float n, float threshhold01) 
{
    return n > threshhold01; //booleans cast automatically to 0 or 1
}

struct VoxelData
{
    uint type;
    float extraDataA;
    float extraDataB;
};

//
// return a VoxelData struct. Voxel type (EmptyVoxel = 0, anything above 0 is a solid voxel.)
// 'extraData a and b' are only used to add colors to the terrain cross-section. (See VoxelDataToPixel)
//
VoxelData Voxel(float3 worldPosition) 
{


    float3 samplePoint = worldPosition * INPUT_SCALE + INPUT_OFFSET;

    //
    // snoise returns a value between -1 and 1
    // that varies smoothly through 3D space (like clouds).
    // When INPUT_SCALE is smaller, 'n' will vary more slowly
    //
    float n = snoise(samplePoint);

    //
    // Call the 2D version of noise. (worldPosition.xz is a float2.)
    // Scale worldPosition.xz by a magic number (TODO: add parameter for this magic number)
    // the return value 'm' varies along x and z but not y. (It's a height map)
    //
    float m = snoise(worldPosition.xz * INPUT_SCALE_2D + INPUT_OFFSET_2D);


    //
    // Determine a test height. If 'H'
    // is below SOLID_TERRAIN_THRESHHOLD, we get a solid voxel.
    // If 'H' was just set to worldPosition.y and not modified, the world 
    // would always be flat.
    // Use 'n' and 'm' to tweak the test height.
    //
    float H = worldPosition.y;

    float crazy = .45; // larger leads to more variation
    
    H += m * GRADIENT_HEIGHT * crazy;
    H += n * GRADIENT_HEIGHT * crazy * .5;

    H = gradient(H, GRADIENT_HEIGHT);
    float t = threshhold(H, SOLID_TERRAIN_THRESHHOLD);

    //Which type of voxel?
    t *= 9; // TODO: voxelType() function. for now it's always 9 (or zero)

    //
    // return a VoxelData
    //
    VoxelData vdata;

    vdata.type = t;
    vdata.extraDataA = .5; //n;
    vdata.extraDataB = m;

    return vdata;
}


//
// Define colors for the terrain cross-section widget.
//
float4 VoxelDataToPixel(VoxelData vdata) 
{
    return float4(vdata.extraDataA /2 + .5, vdata.extraDataB, vdata.type, 1);
}


#endif
