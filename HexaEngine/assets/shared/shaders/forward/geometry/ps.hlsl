#include "defs.hlsl"
#include "../../commonShading.hlsl"

#ifndef BaseColor
#define BaseColor float4(0.8,0.8,0.8,1)
#endif
#ifndef Roughness
#define Roughness 0.8
#endif
#ifndef Metallic
#define Metallic 0
#endif
#ifndef Reflectance
#define Reflectance 0.5
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
#ifndef HasMetallicTex
#define HasMetallicTex 0
#endif
#ifndef HasRoughnessTex
#define HasRoughnessTex 0
#endif
#ifndef HasReflectanceTex
#define HasReflectanceTex 0
#endif
#ifndef HasEmissiveTex
#define HasEmissiveTex 0
#endif
#ifndef HasAmbientOcclusionTex
#define HasAmbientOcclusionTex 0
#endif
#ifndef HasRoughnessMetallicTex
#define HasRoughnessMetallicTex 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetallicTex
#define HasAmbientOcclusionRoughnessMetallicTex 0
#endif

#if HasBaseColorTex
Texture2D baseColorTexture;
SamplerState baseColorTextureSampler;
#endif
#if HasNormalTex
Texture2D normalTexture;
SamplerState normalTextureSampler;
#endif
#if HasRoughnessTex
Texture2D roughnessTexture;
SamplerState roughnessTextureSampler;
#endif
#if HasMetallicTex
Texture2D metallicTexture;
SamplerState metallicTextureSampler;
#endif
#if HasReflectanceTex
Texture2D reflectanceTexture;
SamplerState reflectanceTextureSampler;
#endif
#if HasEmissiveTex
Texture2D emissiveTexture;
SamplerState emissiveTextureSampler;
#endif
#if HasAmbientOcclusionTex
Texture2D ambientOcclusionTexture;
SamplerState ambientOcclusionTextureSampler;
#endif
#if HasRoughnessMetallicTex
Texture2D roughnessMetallicTexture;
SamplerState roughnessMetallicTextureSampler;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex
Texture2D ambientOcclusionRoughnessMetallicTexture;
SamplerState ambientOcclusionRoughnessMetallicSampler;
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

struct PixelOutput
{
    float4 Color : SV_Target0;
    float4 Normal : SV_Target1;
};

[earlydepthstencil]
PixelOutput main(PixelInput input)
{
    float3 position = input.pos;
    
#if VtxColors
    float4 baseColor = input.color;
#else
    float4 baseColor = BaseColor;
#endif

#if VtxNormals
    float3 normal = normalize(input.normal);
#else 
    float3 normal = 0;
#endif

#if VtxTangents
    float3 tangent = normalize(input.tangent);
#else
    float3 tangent = 0;
#endif

    float opacity = 1;

    float ao = Ao;
    float roughness = Roughness;
    float metallic = Metallic;
    float reflectance = Reflectance;
    float3 emissive = Emissive;

#if HasBaseColorTex
	float4 color = baseColorTexture.Sample(baseColorTextureSampler, (float2)input.tex);
	baseColor = float4(color.rgb * color.a, color.a);
#endif

#if HasNormalTex
	normal = NormalSampleToWorldSpace(normalTexture.Sample(normalTextureSampler, (float2)input.tex).rgb, normal, tangent);
#endif

#if HasRoughnessTex
	roughness = roughnessTexture.Sample(roughnessTextureSampler, (float2)input.tex).r;
#endif

#if HasMetallicTex
	metallic = metallicTexture.Sample(metallicTextureSampler, (float2)input.tex).r;
#endif

#if HasReflectanceTex
    reflectance = reflectanceTexture.Sample(reflectanceTextureSampler, (float2) input.tex).r;
#endif

#if HasEmissiveTex
	emissive = emissiveTexture.Sample(emissiveTextureSampler, (float2)input.tex).rgb;
#endif

#if HasAmbientOcclusionTex
	ao = ambientOcclusionTexture.Sample(ambientOcclusionTextureSampler, (float2)input.tex).r;
#endif

#if HasRoughnessMetallicTex
	float2 rm = roughnessMetallicTexture.Sample(roughnessMetallicTextureSampler, (float2)input.tex).gb;
	roughness = rm.x;
	metallic = rm.y;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex
	float3 orm = ambientOcclusionRoughnessMetallicTexture.Sample(ambientOcclusionRoughnessMetallicSampler, (float2)input.tex).rgb;
	ao = orm.r;
	roughness = orm.g;
	metallic = orm.b;
#endif

    if (baseColor.a < 0.1f)
        discard;

    float3 N = normalize(normal);
    float3 V = normalize(GetCameraPos() - position);

    PixelParams pixel = ComputeSurfaceProps(position, V, N, baseColor.rgb, roughness, metallic, reflectance);

    float2 screenUV = GetScreenUV(input.position);

    float3 direct = ComputeDirectLightning(input.position.z / input.position.w, pixel);
    float3 ambient = ComputeIndirectLightning(screenUV, pixel, ao, emissive);

    PixelOutput output;
    output.Color = float4(ambient + direct, baseColor.a);
    output.Normal = float4(PackNormal(N), baseColor.a);

    return output;
}