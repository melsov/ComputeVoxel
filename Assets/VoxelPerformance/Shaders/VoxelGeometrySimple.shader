// VoxelPerformance/Shaders/VoxelGeometry.shader
// Copyright 2016 Charles Griffiths

Shader "VoxelPerformance/VoxelGeometryShaderSimple"
{
	Properties
	{
	  _Sprite("Sprite", 2D) = "white" {}

	  _Size("Size", float) = 1
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

		#define MIP_STRETCH
		#define DEBUG_SHOW_HILBERT_PATTERN

		#define PI 3.14159265359

		sampler2D _Sprite;

		float _Size = 1;

		float4 _cameraPosition, _chunkPosition;


		matrix _worldMatrixTransform;

		struct data
		{
		  float4 pos;
		  float4 color;
		};


		StructuredBuffer<data> _displayPoints;


		  struct input
		  {
			float4 pos : SV_POSITION;
			float4 _color : COLOR;
			float2 uv : TEXCOORD0;
		  };

		  struct inputGS
		  {
			float4 pos : SV_POSITION;
			float4 _color : COLOR;
			float4 uvOffset : TEXCOORD0;

		  };

		  inputGS vert(uint id : SV_VertexID)
		  {
  			inputGS o;
  			o.pos = _displayPoints[id].pos;
  			o._color = _displayPoints[id].color;
  			o.uvOffset = float4(0, 0, 0, 0);
  			return o;
		  }

		  // For each voxel that is visible from some angle, paint the three sides
		  // that the given camera might see.
		  [maxvertexcount(12)]
		  void geom(point inputGS p[1], inout TriangleStream<input> triStream)
		  {
			  //
			  // Original version
			  //
      float4 pos = p[0].pos * float4(_Size, _Size, _Size, 1);
			float4 shift;
			float4 voxelPosition = pos + _chunkPosition;
			float halfS = _Size * 0.5;  // x, y, z is the center of the voxel, paint sides offset by half of Size

			input pIn1, pIn2, pIn3, pIn4;

			  pIn1._color = p[0]._color;
			  pIn1.uv = float2(0.0f, 0.0f) / 4 + p[0].uvOffset;

			  pIn2._color = p[0]._color;
			  pIn2.uv = float2(0.0f, 1.0f) / 4 + p[0].uvOffset;

			  pIn3._color = p[0]._color;
			  pIn3.uv = float2(1.0f, 0.0f) / 4 + p[0].uvOffset;

			  pIn4._color = p[0]._color;
			  pIn4.uv = float2(1.0f, 1.0f) / 4 + p[0].uvOffset;


			  shift = (_cameraPosition.x < voxelPosition.x) ? float4(1, 1, 1, 1) : float4(-1, 1, -1, 1);

			  pIn1.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, -halfS, halfS, 0)));
			  triStream.Append(pIn1);

			  pIn2.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, halfS, halfS, 0)));
			  triStream.Append(pIn2);

			  pIn3.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, -halfS, -halfS, 0)));
			  triStream.Append(pIn3);

			  pIn4.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, halfS, -halfS, 0)));
			  triStream.Append(pIn4);

			  triStream.RestartStrip();

			  // shadows
			  pIn1._color = pIn2._color = pIn3._color = pIn4._color = p[0]._color * .7;

			  shift = (_cameraPosition.y < voxelPosition.y) ? float4(1, 1, 1, 1) : float4(1, -1, -1, 1);

			  pIn1.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, -halfS, halfS, 0)));
			  triStream.Append(pIn1);

			  pIn2.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, -halfS, -halfS, 0)));
			  triStream.Append(pIn2);

			  pIn3.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(halfS, -halfS, halfS, 0)));
			  triStream.Append(pIn3);

			  pIn4.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(halfS, -halfS, -halfS, 0)));
			  triStream.Append(pIn4);

			  triStream.RestartStrip();

			  //side shadows
			  pIn1._color = pIn2._color = pIn3._color = pIn4._color = p[0]._color * .9;

			  shift = (_cameraPosition.z < voxelPosition.z) ? float4(1, 1, 1, 1) : float4(-1, 1, -1, 1);

			  pIn1.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, -halfS, -halfS, 0)));
			  triStream.Append(pIn1);

			  pIn2.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(-halfS, halfS, -halfS, 0)));
			  triStream.Append(pIn2);

			  pIn3.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(halfS, -halfS, -halfS, 0)));
			  triStream.Append(pIn3);

			  pIn4.pos = mul(UNITY_MATRIX_VP, mul(_worldMatrixTransform, pos + shift * float4(halfS, halfS, -halfS, 0)));
			  triStream.Append(pIn4);

			  triStream.RestartStrip();
			}



			float4 frag(input i) : COLOR
			{
				// weird effect
				// i. pos is relative to _cameraPosition ????
				//return float4( (clamp( fmod(i.pos.xyz, 128.0)/256.0 + .5, 0, 1 ) * i._color.g), 1);

				return float4(0, 1, 0, 1); // float4(i._color) *  tex2D( _Sprite, i.uv );
			  }

			ENDCG
			}
	  }

		  Fallback Off
}

