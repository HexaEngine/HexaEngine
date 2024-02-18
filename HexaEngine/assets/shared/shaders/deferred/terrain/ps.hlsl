#include "defs.hlsl"
#include "../../gbuffer.hlsl"

SamplerState maskSamplerState;
Texture2D maskTex;

#ifndef BaseColor0
#define BaseColor0 float3(0.8,0.8,0.8)
#endif
#ifndef BaseColor1
#define BaseColor1 float3(0.8,0.8,0.8)
#endif
#ifndef BaseColor2
#define BaseColor2 float3(0.8,0.8,0.8)
#endif
#ifndef BaseColor3
#define BaseColor3 float3(0.8,0.8,0.8)
#endif

#ifndef Roughness0
#define Roughness0 0.8
#endif
#ifndef Roughness1
#define Roughness1 0.8
#endif
#ifndef Roughness2
#define Roughness2 0.8
#endif
#ifndef Roughness3
#define Roughness3 0.8
#endif

#ifndef Metallic0
#define Metallic0 0
#endif
#ifndef Metallic1
#define Metallic1 0
#endif
#ifndef Metallic2
#define Metallic2 0
#endif
#ifndef Metallic3
#define Metallic3 0
#endif

#ifndef Ao0
#define Ao0 1
#endif
#ifndef Ao1
#define Ao1 1
#endif
#ifndef Ao2
#define Ao2 1
#endif
#ifndef Ao3
#define Ao3 1
#endif

#ifndef Emissive0
#define Emissive0 float3(0,0,0);
#endif
#ifndef Emissive1
#define Emissive1 float3(0,0,0);
#endif
#ifndef Emissive2
#define Emissive2 float3(0,0,0);
#endif
#ifndef Emissive3
#define Emissive3 float3(0,0,0);
#endif

#ifndef HasBaseColorTex0
#define HasBaseColorTex0 0
#endif
#ifndef HasBaseColorTex1
#define HasBaseColorTex1 0
#endif
#ifndef HasBaseColorTex2
#define HasBaseColorTex2 0
#endif
#ifndef HasBaseColorTex3
#define HasBaseColorTex3 0
#endif

#ifndef HasNormalTex0
#define HasNormalTex0 0
#endif
#ifndef HasNormalTex1
#define HasNormalTex1 0
#endif
#ifndef HasNormalTex2
#define HasNormalTex2 0
#endif
#ifndef HasNormalTex3
#define HasNormalTex3 0
#endif

#ifndef HasDisplacementTex0
#define HasDisplacementTex0 0
#endif
#ifndef HasDisplacementTex1
#define HasDisplacementTex1 0
#endif
#ifndef HasDisplacementTex2
#define HasDisplacementTex2 0
#endif
#ifndef HasDisplacementTex3
#define HasDisplacementTex3 0
#endif

#ifndef HasMetallicTex0
#define HasMetallicTex0 0
#endif
#ifndef HasMetallicTex1
#define HasMetallicTex1 0
#endif
#ifndef HasMetallicTex2
#define HasMetallicTex2 0
#endif
#ifndef HasMetallicTex3
#define HasMetallicTex3 0
#endif

#ifndef HasRoughnessTex0
#define HasRoughnessTex0 0
#endif
#ifndef HasRoughnessTex1
#define HasRoughnessTex1 0
#endif
#ifndef HasRoughnessTex2
#define HasRoughnessTex2 0
#endif
#ifndef HasRoughnessTex3
#define HasRoughnessTex3 0
#endif

#ifndef HasEmissiveTex0
#define HasEmissiveTex0 0
#endif
#ifndef HasEmissiveTex1
#define HasEmissiveTex1 0
#endif
#ifndef HasEmissiveTex2
#define HasEmissiveTex2 0
#endif
#ifndef HasEmissiveTex3
#define HasEmissiveTex3 0
#endif

#ifndef HasAmbientOcclusionTex0
#define HasAmbientOcclusionTex0 0
#endif
#ifndef HasAmbientOcclusionTex1
#define HasAmbientOcclusionTex1 0
#endif
#ifndef HasAmbientOcclusionTex2
#define HasAmbientOcclusionTex2 0
#endif
#ifndef HasAmbientOcclusionTex3
#define HasAmbientOcclusionTex3 0
#endif

#ifndef HasRoughnessMetallicTex0
#define HasRoughnessMetallicTex0 0
#endif
#ifndef HasRoughnessMetallicTex1
#define HasRoughnessMetallicTex1 0
#endif
#ifndef HasRoughnessMetallicTex2
#define HasRoughnessMetallicTex2 0
#endif
#ifndef HasRoughnessMetallicTex3
#define HasRoughnessMetallicTex3 0
#endif

#ifndef HasAmbientOcclusionRoughnessMetallicTex0
#define HasAmbientOcclusionRoughnessMetallicTex0 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetallicTex1
#define HasAmbientOcclusionRoughnessMetallicTex1 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetallicTex2
#define HasAmbientOcclusionRoughnessMetallicTex2 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetallicTex3
#define HasAmbientOcclusionRoughnessMetallicTex3 0
#endif

#if HasBaseColorTex0
Texture2D baseColorTexture0;
SamplerState baseColorTextureSampler0;
#endif
#if HasBaseColorTex1
Texture2D baseColorTexture1;
SamplerState baseColorTextureSampler1;
#endif
#if HasBaseColorTex2
Texture2D baseColorTexture2;
SamplerState baseColorTextureSampler2;
#endif
#if HasBaseColorTex3
Texture2D baseColorTexture3;
SamplerState baseColorTextureSampler3;
#endif

#if HasNormalTex0
Texture2D normalTexture0;
SamplerState normalTextureSampler0;
#endif
#if HasNormalTex1
Texture2D normalTexture1;
SamplerState normalTextureSampler1;
#endif
#if HasNormalTex2
Texture2D normalTexture2;
SamplerState normalTextureSampler2;
#endif
#if HasNormalTex3
Texture2D normalTexture3;
SamplerState normalTextureSampler3;
#endif

#if HasRoughnessTex0
Texture2D roughnessTexture0;
SamplerState roughnessTextureSampler0;
#endif
#if HasRoughnessTex1
Texture2D roughnessTexture1;
SamplerState roughnessTextureSampler1;
#endif
#if HasRoughnessTex2
Texture2D roughnessTexture2;
SamplerState roughnessTextureSampler2;
#endif
#if HasRoughnessTex3
Texture2D roughnessTexture3;
SamplerState roughnessTextureSampler3;
#endif

#if HasMetallicTex0
Texture2D metallicTexture0;
SamplerState metallicTextureSampler0;
#endif
#if HasMetallicTex1
Texture2D metallicTexture1;
SamplerState metallicTextureSampler1;
#endif
#if HasMetallicTex2
Texture2D metallicTexture2;
SamplerState metallicTextureSampler2;
#endif
#if HasMetallicTex3
Texture2D metallicTexture3;
SamplerState metallicTextureSampler3;
#endif

#if HasEmissiveTex0
Texture2D emissiveTexture0;
SamplerState emissiveTextureSampler0;
#endif
#if HasEmissiveTex1
Texture2D emissiveTexture1;
SamplerState emissiveTextureSampler1;
#endif
#if HasEmissiveTex2
Texture2D emissiveTexture2;
SamplerState emissiveTextureSampler2;
#endif
#if HasEmissiveTex3
Texture2D emissiveTexture3;
SamplerState emissiveTextureSampler3;
#endif

#if HasAmbientOcclusionTex0
Texture2D ambientOcclusionTexture0;
SamplerState ambientOcclusionTextureSampler0;
#endif
#if HasAmbientOcclusionTex1
Texture2D ambientOcclusionTexture1;
SamplerState ambientOcclusionTextureSampler1;
#endif
#if HasAmbientOcclusionTex2
Texture2D ambientOcclusionTexture2;
SamplerState ambientOcclusionTextureSampler2;
#endif
#if HasAmbientOcclusionTex3
Texture2D ambientOcclusionTexture3;
SamplerState ambientOcclusionTextureSampler3;
#endif

#if HasRoughnessMetallicTex0
Texture2D roughnessMetallicTexture0;
SamplerState roughnessMetallicTextureSampler0;
#endif
#if HasRoughnessMetallicTex1
Texture2D roughnessMetallicTexture1;
SamplerState roughnessMetallicTextureSampler1;
#endif
#if HasRoughnessMetallicTex2
Texture2D roughnessMetallicTexture2;
SamplerState roughnessMetallicTextureSampler2;
#endif
#if HasRoughnessMetallicTex3
Texture2D roughnessMetallicTexture3;
SamplerState roughnessMetallicTextureSampler3;
#endif

#if HasAmbientOcclusionRoughnessMetallicTex0
Texture2D ambientOcclusionRoughnessMetallicTexture0;
SamplerState ambientOcclusionRoughnessMetallicSampler0;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex1
Texture2D ambientOcclusionRoughnessMetallicTexture1;
SamplerState ambientOcclusionRoughnessMetallicSampler1;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex2
Texture2D ambientOcclusionRoughnessMetallicTexture2;
SamplerState ambientOcclusionRoughnessMetallicSampler2;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex3
Texture2D ambientOcclusionRoughnessMetallicTexture3;
SamplerState ambientOcclusionRoughnessMetallicSampler3;
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

GeometryData main(PixelInput input)
{
    float3 baseColor0 = BaseColor0;
    float3 baseColor1 = BaseColor1;
    float3 baseColor2 = BaseColor2;
    float3 baseColor3 = BaseColor3;

    float3 normal = normalize(input.normal);
    float3 normal0 = normal;
    float3 normal1 = normal;
    float3 normal2 = normal;
    float3 normal3 = normal;

    float3 tangent = normalize(input.tangent);

    float3 emissive0 = Emissive0;
    float3 emissive1 = Emissive1;
    float3 emissive2 = Emissive2;
    float3 emissive3 = Emissive3;

    float ao0 = Ao0;
    float ao1 = Ao1;
    float ao2 = Ao2;
    float ao3 = Ao3;
    float roughness0 = Roughness0;
    float roughness1 = Roughness1;
    float roughness2 = Roughness2;
    float roughness3 = Roughness3;
    float metallic0 = Metallic0;
    float metallic1 = Metallic1;
    float metallic2 = Metallic2;
    float metallic3 = Metallic3;

#if HasBaseColorTex0
    float4 color0 = baseColorTexture0.Sample(baseColorTextureSampler0, float2(input.tex.xy));
    baseColor0 = color0.rgb * color0.a;
#endif
#if HasBaseColorTex1
    float4 color1 = baseColorTexture1.Sample(baseColorTextureSampler1, float2(input.tex.xy));
    baseColor1 = color1.rgb * color1.a;
#endif
#if HasBaseColorTex2
    float4 color2 = baseColorTexture2.Sample(baseColorTextureSampler2, float2(input.tex.xy));
    baseColor2 = color2.rgb * color2.a;
#endif
#if HasBaseColorTex3
    float4 color3 = baseColorTexture3.Sample(baseColorTextureSampler3, float2(input.tex.xy));
    baseColor3 = color3.rgb * color3.a;
#endif

#if HasNormalTex0
    normal0 = NormalSampleToWorldSpace(normalTexture0.Sample(normalTextureSampler0, float2(input.tex.xy)).rgb, normal, tangent);
#endif
#if HasNormalTex1
    normal1 = NormalSampleToWorldSpace(normalTexture1.Sample(normalTextureSampler1, float2(input.tex.xy)).rgb, normal, tangent);
#endif
#if HasNormalTex2
    normal2 = NormalSampleToWorldSpace(normalTexture2.Sample(normalTextureSampler2, float2(input.tex.xy)).rgb, normal, tangent);
#endif
#if HasNormalTex3
    normal3 = NormalSampleToWorldSpace(normalTexture3.Sample(normalTextureSampler3, float2(input.tex.xy)).rgb, normal, tangent);
#endif

#if HasRoughnessTex0
    roughness0 = roughnessTexture0.Sample(roughnessTextureSampler0, float2(input.tex.xy)).r;
#endif
#if HasRoughnessTex1
    roughness1 = roughnessTexture1.Sample(roughnessTextureSampler1, float2(input.tex.xy)).r;
#endif
#if HasRoughnessTex2
    roughness2 = roughnessTexture2.Sample(roughnessTextureSampler2, float2(input.tex.xy)).r;
#endif
#if HasRoughnessTex3
    roughness3 = roughnessTexture3.Sample(roughnessTextureSampler3, float2(input.tex.xy)).r;
#endif

#if HasMetallicTex0
    metallic0 = metallicTexture0.Sample(metallicTextureSampler0, float2(input.tex.xy)).r;
#endif
#if HasMetallicTex1
    metallic1 = metallicTexture1.Sample(metallicTextureSampler1, float2(input.tex.xy)).r;
#endif
#if HasMetallicTex2
    metallic2 = metallicTexture2.Sample(metallicTextureSampler2, float2(input.tex.xy)).r;
#endif
#if HasMetallicTex3
    metallic3 = metallicTexture3.Sample(metallicTextureSampler3, float2(input.tex.xy)).r;
#endif

#if HasEmissiveTex0
    emissive0 = emissiveTexture0.Sample(emissiveTextureSampler0, float2(input.tex.xy)).rgb;
#endif
#if HasEmissiveTex1
    emissive1 = emissiveTexture1.Sample(emissiveTextureSampler1, float2(input.tex.xy)).rgb;
#endif
#if HasEmissiveTex2
    emissive2 = emissiveTexture2.Sample(emissiveTextureSampler2, float2(input.tex.xy)).rgb;
#endif
#if HasEmissiveTex3
    emissive3 = emissiveTexture3.Sample(emissiveTextureSampler3, float2(input.tex.xy)).rgb;
#endif

#if HasAmbientOcclusionTex0
    ao0 = ambientOcclusionTexture0.Sample(ambientOcclusionTextureSampler0, float2(input.tex.xy)).r;
#endif
#if HasAmbientOcclusionTex1
    ao1 = ambientOcclusionTexture1.Sample(ambientOcclusionTextureSampler1, float2(input.tex.xy)).r;
#endif
#if HasAmbientOcclusionTex2
    ao2 = ambientOcclusionTexture2.Sample(ambientOcclusionTextureSampler2, float2(input.tex.xy)).r;
#endif
#if HasAmbientOcclusionTex3
    ao3 = ambientOcclusionTexture3.Sample(ambientOcclusionTextureSampler3, float2(input.tex.xy)).r;
#endif

#if HasRoughnessMetallicTex0
    float2 rm0 = roughnessMetallicTexture0.Sample(roughnessMetallicTextureSampler0, float2(input.tex.xy)).gb;
    roughness0 = rm0.x;
    metallic0 = rm0.y;
#endif
#if HasRoughnessMetallicTex1
    float2 rm1 = roughnessMetallicTexture1.Sample(roughnessMetallicTextureSampler1, float2(input.tex.xy)).gb;
    roughness1 = rm1.x;
    metallic1 = rm1.y;
#endif
#if HasRoughnessMetallicTex2
    float2 rm2 = roughnessMetallicTexture2.Sample(roughnessMetallicTextureSampler2, float2(input.tex.xy)).gb;
    roughness2 = rm2.x;
    metallic2 = rm2.y;
#endif
#if HasRoughnessMetallicTex3
    float2 rm3 = roughnessMetallicTexture3.Sample(roughnessMetallicTextureSampler3, float2(input.tex.xy)).gb;
    roughness3 = rm3.x;
    metallic3 = rm3.y;
#endif

#if HasAmbientOcclusionRoughnessMetallicTex0
    float3 orm0 = ambientOcclusionRoughnessMetallicTexture0.Sample(ambientOcclusionRoughnessMetallicSampler0, float2(input.tex.xy)).rgb;
    ao0 = orm0.r;
    roughness0 = orm0.g;
    metallic0 = orm0.b;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex1
    float3 orm1 = ambientOcclusionRoughnessMetallicTexture1.Sample(ambientOcclusionRoughnessMetallicSampler1, float2(input.tex.xy)).rgb;
    ao1 = orm1.r;
    roughness1 = orm1.g;
    metallic1 = orm1.b;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex2
    float3 orm2 = ambientOcclusionRoughnessMetallicTexture2.Sample(ambientOcclusionRoughnessMetallicSampler2, float2(input.tex.xy)).rgb;
    ao2 = orm2.r;
    roughness2 = orm2.g;
    metallic2 = orm2.b;
#endif
#if HasAmbientOcclusionRoughnessMetallicTex3
    float3 orm3 = ambientOcclusionRoughnessMetallicTexture3.Sample(ambientOcclusionRoughnessMetallicSampler3, float2(input.tex.xy)).rgb;
    ao2 = orm2.r;
    roughness2 = orm2.g;
    metallic2 = orm2.b;
#endif

    float4 mask = maskTex.Sample(maskSamplerState, input.ctex).xyzw;

    float opacity = mask.x + mask.y + mask.z + mask.w;

    int matID = -1;

    float3 baseColor = 0;
    baseColor = lerp(baseColor, baseColor0, mask.x);
    baseColor = lerp(baseColor, baseColor1, mask.y);
    baseColor = lerp(baseColor, baseColor2, mask.z);
    baseColor = lerp(baseColor, baseColor3, mask.w);

    normal = 0;
    normal = lerp(normal, normal0, mask.x);
    normal = lerp(normal, normal1, mask.y);
    normal = lerp(normal, normal2, mask.z);
    normal = lerp(normal, normal3, mask.w);

    float roughness = 0;
    roughness = lerp(roughness, roughness0, mask.x);
    roughness = lerp(roughness, roughness1, mask.y);
    roughness = lerp(roughness, roughness2, mask.z);
    roughness = lerp(roughness, roughness3, mask.w);

    float metallic = 0;
    metallic = lerp(metallic, metallic0, mask.x);
    metallic = lerp(metallic, metallic1, mask.y);
    metallic = lerp(metallic, metallic2, mask.z);
    metallic = lerp(metallic, metallic3, mask.w);

    float ao = 0;
    ao = lerp(ao, ao0, mask.x);
    ao = lerp(ao, ao1, mask.y);
    ao = lerp(ao, ao2, mask.z);
    ao = lerp(ao, ao3, mask.w);

    float3 emissive = 0;
    emissive = lerp(emissive, emissive0, mask.x);
    emissive = lerp(emissive, emissive1, mask.y);
    emissive = lerp(emissive, emissive2, mask.z);
    emissive = lerp(emissive, emissive3, mask.w);

    return PackGeometryData(matID, baseColor, normal, roughness, metallic, 0, ao, 0, emissive, 1);
}