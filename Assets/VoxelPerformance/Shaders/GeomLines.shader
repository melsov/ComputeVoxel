Shader "Custom/GeometryShaderTest1"
 {
     Properties
     {
         _Color("Color", Color) = (1,1,1,1)
         _Size("Size", float) = 0.5
     }
     SubShader
     {    
         Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
 
         LOD 100
         Blend SrcAlpha OneMinusSrcAlpha
         // ZWrite Off
         Cull Off
 
         Pass
         {
             CGPROGRAM
             #pragma target 5.0
             #pragma vertex vert
             #pragma geometry geom
             #pragma fragment frag
             #include "UnityCG.cginc"
 
             // Vars
             float4 _Color;
             float _Size;
             float3 _worldPos;
             float3 _camPos;
 
             struct data
             {
                 float4 pos0;
                 float4 color;
             };
     
             StructuredBuffer<data> buf_Points;
 
             struct input
             {
                 float4 pos : SV_POSITION;
                 float4 color: COLOR;
             };
 
             input vert(uint id : SV_VertexID)
             {
                 input o;
             
                 o.pos = float4(buf_Points[id].pos0 + _worldPos, 1.0f);
                 o.color = buf_Points[id].color;
 
                 return o;
             }
             
             [maxvertexcount(4)]
             void geom(line input p[2], inout TriangleStream<input> triStream)
             {
                 float4 s[2];

                 s[0] = p[0].pos;
                 s[1] = p[1].pos;
             

                 s[0].xyz /= s[0].w;
                 s[1].xyz /= s[1].w;
                 s[0].w = s[1].w = 1;
  
                 float4 ab = s[1] - s[0];
                 float4 normal = float4(cross(ab.xyz, (s[0].xyz + ab/2) - _camPos.xyz), 0); 
                 normal = normalize(normal);
         
                 input pIn;
                 pIn.pos = s[0] - normal * _Size;
                 pIn.pos = UnityObjectToClipPos(pIn.pos);
                 pIn.color = p[0].color; // float4(1.0, 0.0, 0.0, 1.0);
                 triStream.Append(pIn);
 
                 pIn.pos = s[0] + normal * _Size;
                 pIn.pos = UnityObjectToClipPos(pIn.pos);                 
                 pIn.color = p[0].color;// float4(1.0, 0.0, 0.0, 1.0);
                 triStream.Append(pIn);
 
                 pIn.pos = s[1] - normal * _Size;
                 pIn.pos = UnityObjectToClipPos(pIn.pos);                 
                 pIn.color = p[1].color; // float4(0.0, 1.0, 0.0, 1.0);
                 triStream.Append(pIn);
 
                 pIn.pos = s[1] + normal * _Size;
                 pIn.pos = UnityObjectToClipPos(pIn.pos);                 
                 pIn.color = p[1].color; //float4(0.0, 1.0, 0.0, 1.0);
                 triStream.Append(pIn);                        
             }
             
             float4 frag(input i) : COLOR
             {
                 return i.color;
             }
 
             ENDCG
         }
     }
 
 FallBack "Diffuse"
 }