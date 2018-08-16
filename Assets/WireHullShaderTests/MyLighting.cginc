
#if !defined(MY_LIGHTING)
#define MY_LIGHTING

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

float4 _Tint;
float _Smoothness;
sampler2D _MainTex;
sampler2D _NormalMap;
float _BumpScale;
// float4 _HeightMap_TexelSize;
float4 _MainTex_ST;
float _Metallic;


struct VertexData {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
};
struct Interpolators {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;

	#if defined(BINORMAL_PER_FRAGMENT)
		float4 tangent : TEXCOORD2;
	#else
		float3 tangent : TEXCOORD2;
		float3 binormal : TEXCOORD3;
	#endif

	float3 worldPos : TEXCOORD4;

	SHADOW_COORDS(5)

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD6;
	#endif



};


void ComputeVertexLightColor(inout Interpolators i) {
	#if defined(VERTEXLIGHT_ON)
		float3 lightPos = float3(
			unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x
		);
		float3 lightVec = lightPos - i.worldPos;
		float3 lightDir = normalize(lightVec);
		float ndotl = DotClamped(i.normal, lightDir);
		float attenuation = 1 / (1 + dot(lightDir, lightDir) * unity_4LightAtten0.x);
		i.vertexLightColor = unity_LightColor[0].rgb * ndotl * attenuation;

//actually
		i.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos, i.normal
		);

	#endif
}


float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}

Interpolators vert(VertexData v)  
{
	Interpolators i;
	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
	i.normal = UnityObjectToWorldNormal(v.normal);

#if defined(BINORMAL_PER_FRAGMENT)
	i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
#else
	i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
	i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
#endif

	TRANSFER_SHADOW(i)

	ComputeVertexLightColor(i);
	return i;
}


UnityLight CreateLight (Interpolators i) {
	UnityLight light;

#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
	light.dir = normalize( _WorldSpaceLightPos0.xyz - i.worldPos);
#else
	light.dir = _WorldSpaceLightPos0.xyz;
#endif


	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);



	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}


UnityIndirect CreateIndirectLight (Interpolators i, float3 viewDir) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

#if defined(VERTEXLIGHT_ON)
	indirectLight.diffuse = i.vertexLightColor;
#endif

#if defined(FORWARD_BASE_PASS)
	indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
	float3 reflectionDir = reflect(-viewDir, i.normal);

	Unity_GlossyEnvironmentData envData;
		envData.roughness = 1 - _Smoothness;
		envData.reflUVW = reflectionDir;
		indirectLight.specular = Unity_GlossyEnvironment(
			UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
		);
#endif

	return indirectLight;
}

void InitializeFragmentNormals(inout Interpolators i) {

	float3 mainNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv), _BumpScale);
	// missing detail normal
	float3 tangentSpaceNormal = mainNormal; 

	#if defined(BINORMAL_PER_FRAGMENT)
		float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
	#else
		float3 binormal = i.binormal;
	#endif

	i.normal = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * binormal +
		tangentSpaceNormal.z * i.normal
	);

}

float4 frag(Interpolators i) : SV_TARGET
{
	InitializeFragmentNormals(i);
	// float3 lightDir = _WorldSpaceLightPos0.xyz;
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	float3 albedo = tex2D(_MainTex, i.uv) * _Tint;
	float3 specularTint;
	float oneMinusReflectivity;
	albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
	//EnergyConservationBetweenDiffuseAndSpecular(albedo, specularTint, oneMinusReflectivity);



	return UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i, viewDir)
		);

}

#endif