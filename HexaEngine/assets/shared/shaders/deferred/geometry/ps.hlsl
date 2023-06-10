#include "defs.hlsl"
#include "../../gbuffer.hlsl"

#ifndef BaseColor
#define BaseColor float4(0.8,0.8,0.8,1)
#endif
#ifndef Roughness
#define Roughness 0.8
#endif
#ifndef Metalness
#define Metalness 0
#endif
#ifndef Specular
#define Specular 0.5
#endif
#ifndef SpecularTint
#define SpecularTint 0
#endif
#ifndef Sheen
#define Sheen 0
#endif
#ifndef SheenTint
#define SheenTint 1
#endif
#ifndef Clearcoat
#define Clearcoat 0
#endif
#ifndef ClearcoatGloss
#define ClearcoatGloss 1
#endif
#ifndef Anisotropic
#define Anisotropic 0
#endif
#ifndef Subsurface
#define Subsurface 0
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

[earlydepthstencil]
GeometryData main(PixelInput input)
{
#if VtxColor
    float4 baseColor = input.color;
#else
    float4 baseColor = BaseColor;
#endif
#if VtxPosition
    float3 pos = (float3) input.pos;
#endif
#if VtxNormal
    float3 normal = normalize(input.normal);
#endif
#if VtxTangent
    float3 tangent = normalize(input.tangent);    
#endif
#if VtxBitangent
    float3 bitangent = normalize(input.bitangent);
#endif
    float3 emissive = Emissive;
    float opacity = 1;

    float ao = Ao;
    float specular = Specular;
    float specularTint = SpecularTint;
    float sheen = Sheen;
    float sheenTint = SheenTint;
    float clearcoat = Clearcoat;
    float clearcoatGloss = ClearcoatGloss;
    float anisotropic = Anisotropic;
    float subsurface = Subsurface;
    float roughness = Roughness;
    float metalness = Metalness;
#if VtxUV
#if HasBaseColorTex
	float4 color = baseColorTexture.Sample(baseColorTextureSampler, (float2) input.tex);
    baseColor = float4(color.rgb * color.a, color.a);
#endif

#if VtxTangent
#if HasNormalTex
#if VtxBitangent
    normal = NormalSampleToWorldSpace(normalTexture.Sample(normalTextureSampler, (float2) input.tex).rgb, normal, tangent, bitangent);
#else
	normal = NormalSampleToWorldSpace(normalTexture.Sample(normalTextureSampler, (float2) input.tex).rgb, normal, tangent);
#endif
#endif
#endif
	
#if HasRoughnessTex
    roughness = roughnessTexture.Sample(roughnessTextureSampler, (float2) input.tex).r;
#endif
	
#if HasMetalnessTex
    metalness = metalnessTexture.Sample(metalnessTextureSampler, (float2) input.tex).r;
#endif
	
#if HasEmissiveTex
    emissive = emissiveTexture.Sample(emissiveTextureSampler, (float2) input.tex).rgb;
#endif
	
#if HasAmbientOcclusionTex
    ao = ambientOcclusionTexture.Sample(ambientOcclusionTextureSampler, (float2) input.tex).r;
#endif
	
#if HasRoughnessMetalnessTex
    float2 rm = roughnessMetalnessTexture.Sample(roughnessMetalnessTextureSampler, (float2) input.tex).gb;
    roughness = rm.x;
    metalness = rm.y;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex
    float3 orm = ambientOcclusionRoughnessMetalnessTexture.Sample(ambientOcclusionRoughnessMetalnessSampler, (float2) input.tex).rgb;
    ao = orm.r;
    roughness = orm.g;
    metalness = orm.b;
#endif
#endif
    if (baseColor.a == 0)
        discard;
#if !VtxTangent
    float3 tangent = float3(0, 1, 0);
#endif

    return PackGeometryData(baseColor.rgb, baseColor.a, pos, 1, normal, roughness, metalness, tangent, emissive, 0, specular, specularTint, ao, 1, anisotropic, 0, clearcoat, clearcoatGloss, 0, 0, sheen, sheenTint);
}