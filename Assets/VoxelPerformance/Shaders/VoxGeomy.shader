Shader "Unlit/VoxGeomy"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct data{
				float4 pos;
			};
			StructuredBuffer<data> buffer;

			struct inputGS
		    {
		        float4 pos : SV_POSITION;
		    };
		    struct input
		    {
		        float4 pos : SV_POSITION;
		        
		    };




			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			inputGS vert (uint id : SV_VertexID)
			{
				inputGS o;
				o.pos = buffer[id].pos;
				return o;
			}

			[maxvertexcount(3)]
			void geom( point inputGS p[1], inout TriangleStream<input> triStream ){
				float4 p0 = float4(0,0,-10,1), p1 = float4(0,0,-10,1), p2 = float4(0,0,-10,1);
				p0.x = -40;
				p1.x = 40;
				p2.y = 40;
				input i0, i1, i2;
				i0.pos = UnityObjectToClipPos(p0);
				i1.pos = UnityObjectToClipPos(p1);
				i2.pos = UnityObjectToClipPos(p2);
				triStream.Append(i0);
				triStream.Append(i1);
				triStream.Append(i2);
				triStream.RestartStrip();
			}
			
			
			fixed4 frag (input i) : COLOR
			{
				return fixed4(1,0,0,1);
			}
			ENDCG
		}
	}
}
