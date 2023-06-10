#include "defs.hlsl"
#include "../../gbuffer.hlsl"

#ifndef BaseColor0
#define BaseColor0 float4(0.8,0.8,0.8,1)
#define BaseColor1 float4(0.8,0.8,0.8,1)
#define BaseColor2 float4(0.8,0.8,0.8,1)
#define BaseColor3 float4(0.8,0.8,0.8,1)
#endif
#ifndef Roughness0
#define Roughness0 0.8
#define Roughness1 0.8
#define Roughness2 0.8
#define Roughness3 0.8
#endif
#ifndef Metalness0
#define Metalness0 0
#define Metalness1 0
#define Metalness2 0
#define Metalness3 0
#endif
#ifndef Specular0
#define Specular0 0.5
#define Specular1 0.5
#define Specular2 0.5
#define Specular3 0.5
#endif
#ifndef SpecularTint0
#define SpecularTint0 0
#define SpecularTint1 0
#define SpecularTint2 0
#define SpecularTint3 0
#endif
#ifndef Sheen0
#define Sheen0 0
#define Sheen1 0
#define Sheen2 0
#define Sheen3 0
#endif
#ifndef SheenTint0
#define SheenTint0 1
#define SheenTint1 0
#define SheenTint2 0
#define SheenTint3 0
#endif
#ifndef Clearcoat0
#define Clearcoat0 0
#define Clearcoat1 0
#define Clearcoat2 0
#define Clearcoat3 0
#endif
#ifndef ClearcoatGloss0
#define ClearcoatGloss0 1
#define ClearcoatGloss1 1
#define ClearcoatGloss2 1
#define ClearcoatGloss3 1
#endif
#ifndef Anisotropic0
#define Anisotropic0 0
#define Anisotropic1 0
#define Anisotropic2 0
#define Anisotropic3 0
#endif
#ifndef Subsurface0
#define Subsurface0 0
#define Subsurface1 0
#define Subsurface2 0
#define Subsurface3 0
#endif
#ifndef Ao0
#define Ao0 1
#define Ao1 1
#define Ao2 1
#define Ao3 1
#endif
#ifndef Emissive0
#define Emissive0 float3(0,0,0);
#define Emissive1 float3(0,0,0);
#define Emissive2 float3(0,0,0);
#define Emissive3 float3(0,0,0);
#endif

#ifndef HasBaseColorTex0
#define HasBaseColorTex0 0
#define HasBaseColorTex1 0
#define HasBaseColorTex2 0
#define HasBaseColorTex3 0
#endif
#ifndef HasNormalTex0
#define HasNormalTex0 0
#define HasNormalTex1 0
#define HasNormalTex2 0
#define HasNormalTex3 0
#endif
#ifndef HasDisplacementTex0
#define HasDisplacementTex0 0
#define HasDisplacementTex1 0
#define HasDisplacementTex2 0
#define HasDisplacementTex3 0
#endif
#ifndef HasMetalnessTex0
#define HasMetalnessTex0 0
#define HasMetalnessTex1 0
#define HasMetalnessTex2 0
#define HasMetalnessTex3 0
#endif
#ifndef HasRoughnessTex0
#define HasRoughnessTex0 0
#define HasRoughnessTex1 0
#define HasRoughnessTex2 0
#define HasRoughnessTex3 0
#endif
#ifndef HasEmissiveTex0
#define HasEmissiveTex0 0
#define HasEmissiveTex1 0
#define HasEmissiveTex2 0
#define HasEmissiveTex3 0
#endif
#ifndef HasAmbientOcclusionTex0
#define HasAmbientOcclusionTex0 0
#define HasAmbientOcclusionTex1 0
#define HasAmbientOcclusionTex2 0
#define HasAmbientOcclusionTex3 0
#endif
#ifndef HasRoughnessMetalnessTex0
#define HasRoughnessMetalnessTex0 0
#define HasRoughnessMetalnessTex1 0
#define HasRoughnessMetalnessTex2 0
#define HasRoughnessMetalnessTex3 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetalnessTex0
#define HasAmbientOcclusionRoughnessMetalnessTex0 0
#define HasAmbientOcclusionRoughnessMetalnessTex1 0
#define HasAmbientOcclusionRoughnessMetalnessTex2 0
#define HasAmbientOcclusionRoughnessMetalnessTex3 0
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

SamplerState state;
Texture2DArray colorTex;
Texture2D maskTex;

GeometryData main(PixelInput input)
{
    float4 baseColor0 = BaseColor0;
    float4 baseColor1 = BaseColor1;
    float4 baseColor2 = BaseColor2;
    float4 baseColor3 = BaseColor3;

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
    float3 emissive0 = Emissive0;
    float3 emissive1 = Emissive1;
    float3 emissive2 = Emissive2;
    float3 emissive3 = Emissive3;

    float ao0 = Ao0;
    float ao1 = Ao1;
    float ao2 = Ao2;
    float ao3 = Ao3;
    float specular0 = Specular0;
    float specular1 = Specular1;
    float specular2 = Specular2;
    float specular3 = Specular3;
    float specularTint0 = SpecularTint0;
    float specularTint1 = SpecularTint1;
    float specularTint2 = SpecularTint2;
    float specularTint3 = SpecularTint3;
    float sheen0 = Sheen0;
    float sheen1 = Sheen1;
    float sheen2 = Sheen2;
    float sheen3 = Sheen3;
    float sheenTint0 = SheenTint0;
    float sheenTint1 = SheenTint1;
    float sheenTint2 = SheenTint2;
    float sheenTint3 = SheenTint3;
    float clearcoat0 = Clearcoat0;
    float clearcoat1 = Clearcoat1;
    float clearcoat2 = Clearcoat2;
    float clearcoat3 = Clearcoat3;
    float clearcoatGloss0 = ClearcoatGloss0;
    float clearcoatGloss1 = ClearcoatGloss1;
    float clearcoatGloss2 = ClearcoatGloss2;
    float clearcoatGloss3 = ClearcoatGloss3;
    float anisotropic0 = Anisotropic0;
    float anisotropic1 = Anisotropic1;
    float anisotropic2 = Anisotropic2;
    float anisotropic3 = Anisotropic3;
    float subsurface0 = Subsurface0;
    float subsurface1 = Subsurface1;
    float subsurface2 = Subsurface2;
    float subsurface3 = Subsurface3;
    float roughness0 = Roughness0;
    float roughness1 = Roughness1;
    float roughness2 = Roughness2;
    float roughness3 = Roughness3;
    float metalness0 = Metalness0;
    float metalness1 = Metalness1;
    float metalness2 = Metalness2;
    float metalness3 = Metalness3;
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

#if !VtxTangent
    float3 tangent = float3(0, 1, 0);
#endif


    float3 c0 = colorTex.Sample(state, input.tex).xyz;
    float3 c1 = colorTex.Sample(state, input.tex).xyz;
    float3 c2 = colorTex.Sample(state, input.tex).xyz;

	float4 mask = maskTex.Sample(state, input.ctex).xyzw;

	float3 color = float3(0.0f, 0.0f, 0.0f);
	color = lerp(color, c0, mask.r);
	color = lerp(color, c1, mask.g);
	color = lerp(color, c2, mask.b);

    float opacity = mask.x + mask.y + mask.z + mask.w;
    
    int matID = -1;

    return PackGeometryData(matID, baseColor0.rgb, normal, roughness0, metalness0, 0, ao0, 0, emissive0, 1);
}