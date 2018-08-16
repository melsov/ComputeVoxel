// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MyShader"
{
	Properties {
		_Tint ("_Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo", 2D) = "white" {}
		[NoScaleOffset] _NormalMap ("Normals", 2D) = "gray" {}
		_BumpScale ("Bump Scale", Float) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
	}

	CGINCLUDE

	#define BINORMAL_PER_FRAGMENT

	ENDCG

	SubShader {


		Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}
			CGPROGRAM
			#pragma target 3.0

			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON

			#define FORWARD_BASE_PASS

			#pragma vertex vert
			#pragma fragment frag

			#include "MyLighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM
			#pragma target 3.0

			#pragma multi_compile_fwdadd_fullshadows
			// #pragma multi_compile DIRECTIONAL POINT SPOT DIRECTIONAL_COOKIE

			#pragma vertex vert
			#pragma fragment frag



			#include "MyLighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#include "MyShadows.cginc"

			ENDCG
		}

	}
}
