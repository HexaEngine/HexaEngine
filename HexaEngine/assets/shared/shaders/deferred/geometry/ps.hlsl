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
#ifndef HasAoTex
#define HasAoTex 0
#endif
#ifndef HasRoughnessMetalnessTex
#define HasRoughnessMetalnessTex 0
#endif

#if (DEPTH != 1)
Texture2D albedoTexture : register(t0);
Texture2D normalTexture : register(t1);
Texture2D roughnessTexture : register(t2);
Texture2D metalnessTexture : register(t3);
Texture2D emissiveTexture : register(t4);
Texture2D aoTexture : register(t5);
Texture2D rmTexture : register(t6);

SamplerState materialSamplerState : register(s0);

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

#endif

#if (DEPTH == 1)

float4 main(PixelInput input) : SV_Target
{
	return float4(input.depth, input.depth, input.depth, 1.0f);
}

#else

GeometryData main(PixelInput input)
{
    float4 baseColor = BaseColor;
    float3 pos = (float3) input.pos;
    float3 normal = normalize(input.normal);
    float3 tangent = normalize(input.tangent);
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

#if HasBaseColorTex
	float4 color = albedoTexture.Sample(materialSamplerState, (float2) input.tex);
    baseColor = float4(color.rgb * color.a, color.a);
#endif

#if HasNormalTex
	normal = NormalSampleToWorldSpace(normalTexture.Sample(materialSamplerState, (float2) input.tex).rgb, normal, tangent);
#endif
	
#if HasRoughnessTex
	roughness = roughnessTexture.Sample(materialSamplerState, (float2) input.tex).r;
#endif
	
#if HasMetalnessTex
	metalness = metalnessTexture.Sample(materialSamplerState, (float2) input.tex).r;
#endif
	
#if HasEmissiveTex
	emissive = emissiveTexture.Sample(materialSamplerState, (float2) input.tex).rgb;
#endif
	
#if HasAoTex
	ao = aoTexture.Sample(materialSamplerState, (float2) input.tex).r;
#endif
	
#if HasRoughnessMetalnessTex
	roughness = rmTexture.Sample(materialSamplerState, (float2) input.tex).g;
	metalness = rmTexture.Sample(materialSamplerState, (float2) input.tex).b;
#endif

    if (baseColor.a == 0)
        discard;

    return PackGeometryData(baseColor.rgb, baseColor.a, pos, input.depth, normal, roughness, metalness, tangent, emissive, 0, specular, specularTint, ao, 1, anisotropic, 0, clearcoat, clearcoatGloss, 0, 0, sheen, sheenTint);
}

#endif