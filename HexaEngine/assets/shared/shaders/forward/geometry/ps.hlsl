#include "defs.hlsl"
#include "../../light.hlsl"
#include "../../camera.hlsl"
#include "../../shadow.hlsl"
#include "../../weather.hlsl"
#include "../../gbuffer.hlsl"

#if CLUSTERED_FORWARD
#include "../../cluster.hlsl"
#endif

#ifndef BaseColor
#define BaseColor float4(0.8,0.8,0.8,1)
#endif
#ifndef Roughness
#define Roughness 0.8
#endif
#ifndef Metalness
#define Metalness 0
#endif
#ifndef Ao
#define Ao 1
#endif
#ifndef Emissive
#define Emissive float3(0,0,0);
#endif

#ifndef HasBaseColorTex
#define HasBaseColorTex 0
#endif
#ifndef HasNormalTex
#define HasNormalTex 0
#endif
#ifndef HasDisplacementTex
#define HasDisplacementTex 0
#endif
#ifndef HasMetalnessTex
#define HasMetalnessTex 0
#endif
#ifndef HasRoughnessTex
#define HasRoughnessTex 0
#endif
#ifndef HasEmissiveTex
#define HasEmissiveTex 0
#endif
#ifndef HasAmbientOcclusionTex
#define HasAmbientOcclusionTex 0
#endif
#ifndef HasRoughnessMetalnessTex
#define HasRoughnessMetalnessTex 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetalnessTex
#define HasAmbientOcclusionRoughnessMetalnessTex 0
#endif

#if HasBaseColorTex
Texture2D baseColorTexture : register(t0);
SamplerState baseColorTextureSampler : register(s0);
#endif
#if HasNormalTex
Texture2D normalTexture : register(t1);
SamplerState normalTextureSampler : register(s1);
#endif
#if HasRoughnessTex
Texture2D roughnessTexture : register(t2);
SamplerState roughnessTextureSampler : register(s2);
#endif
#if HasMetalnessTex
Texture2D metalnessTexture : register(t3);
SamplerState metalnessTextureSampler : register(s3);
#endif
#if HasEmissiveTex
Texture2D emissiveTexture : register(t4);
SamplerState emissiveTextureSampler : register(s4);
#endif
#if HasAmbientOcclusionTex
Texture2D ambientOcclusionTexture : register(t5);
SamplerState ambientOcclusionTextureSampler : register(s5);
#endif
#if HasRoughnessMetalnessTex
Texture2D roughnessMetalnessTexture : register(t6);
SamplerState roughnessMetalnessTextureSampler : register(s6);
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex
Texture2D ambientOcclusionRoughnessMetalnessTexture : register(t7);
SamplerState ambientOcclusionRoughnessMetalnessSampler : register(s7);
#endif

Texture2D ssao : register(t8);
Texture2D brdfLUT : register(t9);

StructuredBuffer<GlobalProbe> globalProbes : register(t10);
StructuredBuffer<Light> lights : register(t11);
StructuredBuffer<ShadowData> shadowData : register(t12);

#if !CLUSTERED_FORWARD
Texture2D depthAtlas : register(t13);
Texture2DArray depthCSM : register(t14);

TextureCube globalDiffuse : register(t15);
TextureCube globalSpecular : register(t16);
#endif

#if CLUSTERED_FORWARD
StructuredBuffer<uint> lightIndexList : register(t13); //MAX_CLUSTER_LIGHTS * 16^3
StructuredBuffer<LightGrid> lightGrid : register(t14); //16^3

Texture2D depthAtlas : register(t15);
Texture2DArray depthCSM : register(t16);

TextureCube globalDiffuse : register(t17);
TextureCube globalSpecular : register(t18);
#endif

SamplerState linearClampSampler : register(s8);
SamplerState linearWrapSampler : register(s9);
SamplerState pointClampSampler : register(s10);
SamplerComparisonState shadowSampler : register(s11);

#if !CLUSTERED_FORWARD
cbuffer constants : register(b0)
{
	uint cLightCount;
};
#endif

float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW, float3 bitangent)
{
	// Uncompress each component from [0,1] to [-1,1].
	float3 normalT = 2.0f * normalMapSample - 1.0f;

	// Build orthonormal basis.
	float3 N = unitNormalW;
	float3 T = normalize(tangentW - dot(tangentW, N) * N);
	float3 B = cross(N, T);

	float3x3 TBN = float3x3(T, B, N);

	// Transform from tangent space to world space.
	float3 bumpedNormalW = mul(normalT, TBN);

	return bumpedNormalW;
}

float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW)
{
	// Uncompress each component from [0,1] to [-1,1].
	float3 normalT = 2.0f * normalMapSample - 1.0f;

	// Build orthonormal basis.
	float3 N = unitNormalW;
	float3 T = normalize(tangentW - dot(tangentW, N) * N);
	float3 B = cross(N, T);

	float3x3 TBN = float3x3(T, B, N);

	// Transform from tangent space to world space.
	float3 bumpedNormalW = mul(normalT, TBN);

	return bumpedNormalW;
}

float ShadowFactorSpotlight(ShadowData data, float3 position, SamplerComparisonState state)
{
	float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

#if HARD_SHADOWS_SPOTLIGHTS
	return CalcShadowFactor_Basic(state, depthAtlas, uvd);
#else
	return CalcShadowFactor_PCF3x3(state, depthAtlas, uvd, data.size, data.softness);
#endif
}

// array of offset direction for sampling
static const float3 gridSamplingDisk[20] =
{
	float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
	float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
	float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
	float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
	float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

#define HARD_SHADOWS_POINTLIGHTS 0
float ShadowFactorPointLight(ShadowData data, Light light, float3 position, SamplerComparisonState state)
{
	float3 lightDirection = position - light.position.xyz;

	int face = GetPointLightFace(lightDirection);
	float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[face], data.views[face]);

#if HARD_SHADOWS_POINTLIGHTS
	return CalcShadowFactor_Basic(state, depthAtlas, uvd);
#else
	return CalcShadowFactor_Poisson(state, depthAtlas, uvd, data.size, data.softness);
#endif
}

float ShadowFactorDirectionalLight(ShadowData data, float3 position, Texture2D depthTex, SamplerComparisonState state)
{
	float3 uvd = GetShadowUVD(position, data.views[0]);

#if HARD_SHADOWS_DIRECTIONAL
	return CalcShadowFactor_Basic(state, depthTex, uvd);
#else
	return CalcShadowFactor_PCF3x3(state, depthTex, uvd, data.size, 1);
#endif
}

float ShadowFactorDirectionalLightCascaded(ShadowData data, float camFar, float4x4 camView, float3 position, Texture2DArray depthTex, SamplerComparisonState state)
{
	float cascadePlaneDistances[8] = (float[8]) data.cascades;
	float farPlane = camFar;

	// select cascade layer
	float4 fragPosViewSpace = mul(float4(position, 1.0), camView);
	float depthValue = abs(fragPosViewSpace.z);
	float cascadePlaneDistance;
	uint layer = data.cascadeCount;
	for (uint i = 0; i < data.cascadeCount; ++i)
	{
		if (depthValue < cascadePlaneDistances[i])
		{
			cascadePlaneDistance = cascadePlaneDistances[i];
			layer = i;
			break;
		}
	}

	float3 uvd = GetShadowUVD(position, data.views[layer]);

#if HARD_SHADOWS_DIRECTIONAL_CASCADED
	return CSMCalcShadowFactor_Basic(state, depthTex, layer, uvd, data.size, data.softness);
#else
	return CSMCalcShadowFactor_PCF3x3(state, depthTex, layer, uvd, data.size, data.softness);
#endif
}

float3 GetHBasisIrradiance(in float3 n, in float3 H0, in float3 H1, in float3 H2, in float3 H3)
{
	float3 color = 0.0f;

	// Band 0
	color += H0 * (1.0f / sqrt(2.0f * 3.14159f));

	// Band 1
	color += H1 * -sqrt(1.5f / 3.14159f) * n.y;
	color += H2 * sqrt(1.5f / 3.14159f) * (2 * n.z - 1.0f);
	color += H3 * -sqrt(1.5f / 3.14159f) * n.x;

	return color;
}

struct Pixel
{
	float4 Color : SV_Target0;
	float4 Normal : SV_Target1;
};

[earlydepthstencil]
Pixel main(PixelInput input)
{
	float3 position = input.pos;
	float4 baseColor = BaseColor;
	float3 normal = normalize(input.normal);
	float3 tangent = normalize(input.tangent);
	float3 bitangent = normalize(input.bitangent);

	float opacity = 1;

	float ao = Ao;
	float roughness = Roughness;
	float metallic = Metalness;
	float3 emissive = Emissive;

#if HasBaseColorTex
	float4 color = baseColorTexture.Sample(baseColorTextureSampler, (float2)input.tex);
	baseColor = float4(color.rgb * color.a, color.a);
#endif

#if HasNormalTex
	normal = NormalSampleToWorldSpace(normalTexture.Sample(normalTextureSampler, (float2)input.tex).rgb, normal, tangent, bitangent);
#endif

#if HasRoughnessTex
	roughness = roughnessTexture.Sample(roughnessTextureSampler, (float2)input.tex).r;
#endif

#if HasMetalnessTex
	metallic = metalnessTexture.Sample(metalnessTextureSampler, (float2)input.tex).r;
#endif

#if HasEmissiveTex
	emissive = emissiveTexture.Sample(emissiveTextureSampler, (float2)input.tex).rgb;
#endif

#if HasAmbientOcclusionTex
	ao = ambientOcclusionTexture.Sample(ambientOcclusionTextureSampler, (float2)input.tex).r;
#endif

#if HasRoughnessMetalnessTex
	float2 rm = roughnessMetalnessTexture.Sample(roughnessMetalnessTextureSampler, (float2)input.tex).gb;
	roughness = rm.x;
	metallic = rm.y;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex
	float3 orm = ambientOcclusionRoughnessMetalnessTexture.Sample(ambientOcclusionRoughnessMetalnessSampler, (float2)input.tex).rgb;
	ao = orm.r;
	roughness = orm.g;
	metallic = orm.b;
#endif

	if (baseColor.a < 0.1f)
		discard;

	float3 N = normalize(normal);
	float3 V = normalize(GetCameraPos() - position);

	float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor.rgb, metallic);

	float3 Lo = float3(0, 0, 0);

#if CLUSTERED_FORWARD
	uint tileIndex = GetClusterIndex(GetLinearDepth(input.position.z / input.position.w), camNear, camFar, screenDim, float4(position, 1));

	uint lightCount = lightGrid[tileIndex].lightCount;
	uint lightOffset = lightGrid[tileIndex].lightOffset;
#else
	uint lightCount = cLightCount;
#endif

	[loop]
		for (uint i = 0; i < lightCount; i++)
		{
			float3 L = 0;
#if CLUSTERED_FORWARD
			uint lightIndex = lightIndexList[lightOffset + i];
			Light light = lights[lightIndex];
#else
			Light light = lights[i];
#endif

			switch (light.type)
			{
			case POINT_LIGHT:
				L += PointLightBRDF(light, position, F0, V, N, baseColor.rgb, roughness, metallic);
				break;
			case SPOT_LIGHT:
				L += SpotlightBRDF(light, position, F0, V, N, baseColor.rgb, roughness, metallic);
				break;
			case DIRECTIONAL_LIGHT:
				L += DirectionalLightBRDF(light, F0, V, N, baseColor.rgb, roughness, metallic);
				break;
			}

			float shadowFactor = 1;

			if (light.castsShadows)
			{
				ShadowData data = shadowData[light.shadowMapIndex];
				switch (light.type)
				{
				case POINT_LIGHT:
					shadowFactor = ShadowFactorPointLight(data, light, position, shadowSampler);
					break;
				case SPOT_LIGHT:
					shadowFactor = ShadowFactorSpotlight(data, position, shadowSampler);
					break;
				case DIRECTIONAL_LIGHT:
					shadowFactor = ShadowFactorDirectionalLightCascaded(data, camFar, view, position, depthCSM, shadowSampler);
					break;
				}
			}

			Lo += L * shadowFactor;
		}

	float3 ambient = baseColor * ambient_color;

	float2 screenUV = GetScreenUV(input.position);

	ao *= ssao.Sample(linearClampSampler, screenUV);

	ambient = (ambient + BRDF_IBL(linearWrapSampler, globalDiffuse, globalSpecular, brdfLUT, F0, N, V, baseColor.xyz, roughness)) * ao;
	ambient += emissive;

#if HasBakedLightMap
	ambient += GetHBasisIrradiance(normal, input.H0, input.H1, input.H2, input.H3) / 3.14159f;
#endif

	Pixel output;
	output.Color = float4(ambient + Lo, baseColor.a);
	output.Normal = float4(PackNormal(N), baseColor.a);

#if BAKE_FORWARD
	// For baking we render without backface culling, so that we still get occlusions inside meshes that
	// don't have full modeled interiors. If it is a backface triangle, we output pure black.
	output.Color = input.IsFrontFace ? output.Color : 0.0f;
#endif

	return output;
}