// VoxelPerformance/Shaders/VoxelGeometry.shader
// Copyright 2016 Charles Griffiths

Shader "VoxelPerformance/VoxelGeometryShader" 
{
  Properties
  {
    _Sprite( "Sprite", 2D ) = "white" {}

    _Size( "Size", float ) = 1
  }


  SubShader
  {
    Tags{ "Queue" = "Geometry" "RenderType" = "Transparent" }

    Pass
    {

     
    CGPROGRAM
      #pragma target 5.0

      #pragma vertex vert
      #pragma geometry geom
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "ChunkConstants.cginc"
      #include "VoxGeomHelper.cginc"


      #define PI 3.14159265359

      sampler2D _Sprite;

      float _Size = 1;

      float4 _cameraPosition, _chunkPosition;

      float3 _globalLight;
      fixed _minAmbianLight = .75;

      float _mipStretch;

      matrix _worldMatrixTransform;

        

      StructuredBuffer<GeomVoxelData> _displayPoints;
      StructuredBuffer<GeomVoxelData> _displayPointsLOD2;
      StructuredBuffer<GeomVoxelData> _displayPointsLOD4;

      


      GeomVoxelData GetVoxelAtMip(uint index)
      {
        if(_mipStretch < 1.1) 
        {
          return _displayPoints[index];
        }

        if(_mipStretch < 2.1) 
        {
          return _displayPointsLOD2[index];
        }

        return _displayPointsLOD4[index];
      }

      inputGS vert( uint id : SV_VertexID )
      {
        inputGS o;
       
        GeomVoxelData data = GetVoxelAtMip(id);
        uint voxel = data.voxel; 

        //
        // The four bytes in voxel are: type, x, y, z.
        // They are packed into a 32 bit uint
        //
        o.pos = float4(voxel/65536 % CHUNK_DIM_X, voxel/256 % CHUNK_DIM_Y, voxel % CHUNK_DIM_Z, 1.0f ); // 65536 = 256 squared
        uint type = voxel/16777216; // 16777216 = 256 cubed


        //DEBUG
        type += _chunkPosition.y % 8;
        //
        // Center the cube if it's larger than 1x1x1. (does nothing if _mipStretch == 1)
        // fmod is short for 'float mod'. 
        // fmod(something, 1) will be zero for any something that doesn't have a fractional component. 
        //
        o.pos.xyz = o.pos.xyz - fmod(o.pos.xyz, _mipStretch) + (_mipStretch/2 - .5);

        //
        // NeighborBits12
        //
        float neiBits12 = data.extras;
        o.extras = uint4(neiBits12, neiBits12, neiBits12, neiBits12);

        o._color = float4(1,1,1, neiBits12); 

        o.uvOffset = float4(0, 0, 0, 0);
        o.uvOffset.x = ((type - 1) % 4) * .25;
        o.uvOffset.y = ((type - 1) / 4) * .25;

        return o;
      }
      

      [maxvertexcount(12)]
      void geom( point inputGS p[1], inout TriangleStream<input> triStream )
      {
             
        float4 pos = p[0].pos * float4( _Size, _Size, _Size, 1 );

        float4 shift;
        float4 voxelPosition = pos + _chunkPosition;

        float halfS = _Size * 0.5 * _mipStretch;  // x, y, z is the center of the voxel, paint sides offset by half of Size


        input pIn1, pIn2, pIn3, pIn4;

        pIn1.uv = float2( 0.0f, 0.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;
        pIn2.uv = float2( 0.0f, 1.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;
        pIn3.uv = float2( 1.0f, 0.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;
        pIn4.uv = float2( 1.0f, 1.0f ) / TEX_TILE_SCALE + p[0].uvOffset.xy;


        // save original color data
        uint4 extras = p[0].extras; //_color;

        
        //
        // global light calcluation
        //
        fixed3 voxLessThanCam = step( voxelPosition.xyz, _cameraPosition.xyz);
        fixed3 corner = voxLessThanCam - (1 - voxLessThanCam);
        fixed3 xNorm = fixed3(corner.x, 0, 0);
        fixed3 yNorm = fixed3(0, corner.y, 0);
        fixed3 zNorm = fixed3(0, 0, corner.z);

        // float3 fakeLightGlobalPos = float3(1000, 1000, 300);
        fixed3 lightDir = normalize(voxelPosition.xyz - _globalLight); // fakeLightGlobalPos);


        fixed3 xyzLight = fixed3(dot(lightDir, xNorm), dot(lightDir, yNorm), dot(lightDir, zNorm)) * -1;
        xyzLight = 1; // TEST--WANT-->  (1 - _minAmbianLight) * (xyzLight / 2 + .5) + _minAmbianLight;


        shift = (_cameraPosition.x < voxelPosition.x)?float4( 1, 1, 1, 1 ):float4( -1, 1, -1, 1 );

        pIn1._color = pIn2._color = pIn3._color = pIn4._color = float4(1, .9, .9, 1); // p[0]._color * xyzLight[0]; //1

        pIn1 = GetVertData(pIn1, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_X, 0 );
        triStream.Append( pIn1 );

        pIn2 = GetVertData(pIn2, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_X, 1 );
        triStream.Append( pIn2 );

        pIn3 = GetVertData(pIn3, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_X, 2 );
        triStream.Append( pIn3 );

        pIn4 = GetVertData(pIn4, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_X, 3 );
        triStream.Append( pIn4 );

        triStream.RestartStrip();


        // shadows
        pIn1._color = pIn2._color = pIn3._color = pIn4._color = float4(.9, 1, .9, 1); // p[0]._color * xyzLight.y; // .7;

        shift = (_cameraPosition.y < voxelPosition.y)?float4( 1, 1, 1, 1 ):float4( 1, -1, -1, 1 );

        pIn1 = GetVertData(pIn1, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Y, 0 );
        triStream.Append( pIn1 );

        pIn2 = GetVertData(pIn2, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Y, 1 );
        triStream.Append( pIn2 );

        pIn3 = GetVertData(pIn3, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Y, 2 );
        triStream.Append( pIn3 );

        pIn4 = GetVertData(pIn4, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Y, 3 );
        triStream.Append( pIn4 );

        triStream.RestartStrip();


        //side shadows
        pIn1._color = pIn2._color = pIn3._color = pIn4._color = float4(.9, .9, 1, 1); // p[0]._color * xyzLight.z; // .9;

        shift = (_cameraPosition.z < voxelPosition.z)?float4( 1, 1, 1, 1 ):float4( -1, 1, -1, 1 );

        pIn1 = GetVertData(pIn1, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Z, 0 );
        triStream.Append( pIn1 );

        pIn2 = GetVertData(pIn2, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Z, 1 );
        triStream.Append( pIn2 );

        pIn3 = GetVertData(pIn3, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Z, 2 );
        triStream.Append( pIn3 );

        pIn4 = GetVertData(pIn4, _worldMatrixTransform, pos, extras, shift, halfS, FACES_OFFSET_Z, 3 );
        triStream.Append( pIn4 );

        triStream.RestartStrip();
      }



      float4 frag( input i ) : COLOR
      {
        return float4(i._color); // want --> * tex2D( _Sprite, i.uv );
      }

    ENDCG
    }
  }

  Fallback Off
}

