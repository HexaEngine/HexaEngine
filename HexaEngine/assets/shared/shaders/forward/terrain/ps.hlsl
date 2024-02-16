#include "defs.hlsl"
#include "../../light.hlsl"
#include "../../camera.hlsl"
#include "../../shadowCommon.hlsl"
#include "../../weather.hlsl"
#include "../../gbuffer.hlsl"

#if CLUSTERED_FORWARD
#include "../../cluster.hlsl"
#endif

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

#ifndef Metalness0
#define Metalness0 0
#endif
#ifndef Metalness1
#define Metalness1 0
#endif
#ifndef Metalness2
#define Metalness2 0
#endif
#ifndef Metalness3
#define Metalness3 0
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

#ifndef HasMetalnessTex0
#define HasMetalnessTex0 0
#endif
#ifndef HasMetalnessTex1
#define HasMetalnessTex1 0
#endif
#ifndef HasMetalnessTex2
#define HasMetalnessTex2 0
#endif
#ifndef HasMetalnessTex3
#define HasMetalnessTex3 0
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

#ifndef HasRoughnessMetalnessTex0
#define HasRoughnessMetalnessTex0 0
#endif
#ifndef HasRoughnessMetalnessTex1
#define HasRoughnessMetalnessTex1 0
#endif
#ifndef HasRoughnessMetalnessTex2
#define HasRoughnessMetalnessTex2 0
#endif
#ifndef HasRoughnessMetalnessTex3
#define HasRoughnessMetalnessTex3 0
#endif

#ifndef HasAmbientOcclusionRoughnessMetalnessTex0
#define HasAmbientOcclusionRoughnessMetalnessTex0 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetalnessTex1
#define HasAmbientOcclusionRoughnessMetalnessTex1 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetalnessTex2
#define HasAmbientOcclusionRoughnessMetalnessTex2 0
#endif
#ifndef HasAmbientOcclusionRoughnessMetalnessTex3
#define HasAmbientOcclusionRoughnessMetalnessTex3 0
#endif

SamplerState linearClampSampler : register(s0);
SamplerState linearWrapSampler : register(s1);
SamplerState pointClampSampler : register(s2);
SamplerComparisonState shadowSampler : register(s3);

Texture2D ssao : register(t0);
Texture2D brdfLUT : register(t1);

StructuredBuffer<GlobalProbe> globalProbes : register(t2);
StructuredBuffer<Light> lights : register(t3);
StructuredBuffer<ShadowData> shadowData : register(t4);

#if !CLUSTERED_FORWARD
Texture2D depthAtlas : register(t5);
Texture2DArray depthCSM : register(t6);

TextureCube globalDiffuse : register(t7);
TextureCube globalSpecular : register(t8);
#endif

#if CLUSTERED_FORWARD
StructuredBuffer<uint> lightIndexList : register(t5); //MAX_CLUSTER_LIGHTS * 16^3
StructuredBuffer<LightGrid> lightGrid : register(t6); //16^3

Texture2D depthAtlas : register(t7);
Texture2DArray depthCSM : register(t8);

TextureCube globalDiffuse : register(t9);
TextureCube globalSpecular : register(t10);
#endif

Texture2D maskTex : register(t11);

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

#if HasMetalnessTex0
Texture2D metalnessTexture0;
SamplerState metalnessTextureSampler0;
#endif
#if HasMetalnessTex1
Texture2D metalnessTexture1;
SamplerState metalnessTextureSampler1;
#endif
#if HasMetalnessTex2
Texture2D metalnessTexture2;
SamplerState metalnessTextureSampler2;
#endif
#if HasMetalnessTex3
Texture2D metalnessTexture3;
SamplerState metalnessTextureSampler3;
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

#if HasRoughnessMetalnessTex0
Texture2D roughnessMetalnessTexture0;
SamplerState roughnessMetalnessTextureSampler0;
#endif
#if HasRoughnessMetalnessTex1
Texture2D roughnessMetalnessTexture1;
SamplerState roughnessMetalnessTextureSampler1;
#endif
#if HasRoughnessMetalnessTex2
Texture2D roughnessMetalnessTexture2;
SamplerState roughnessMetalnessTextureSampler2;
#endif
#if HasRoughnessMetalnessTex3
Texture2D roughnessMetalnessTexture3;
SamplerState roughnessMetalnessTextureSampler3;
#endif

#if HasAmbientOcclusionRoughnessMetalnessTex0
Texture2D ambientOcclusionRoughnessMetalnessTexture0;
SamplerState ambientOcclusionRoughnessMetalnessSampler0;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex1
Texture2D ambientOcclusionRoughnessMetalnessTexture1;
SamplerState ambientOcclusionRoughnessMetalnessSampler1;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex2
Texture2D ambientOcclusionRoughnessMetalnessTexture2;
SamplerState ambientOcclusionRoughnessMetalnessSampler2;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex3
Texture2D ambientOcclusionRoughnessMetalnessTexture3;
SamplerState ambientOcclusionRoughnessMetalnessSampler3;
#endif

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

Pixel main(PixelInput input)
{
    float3 position = input.pos;
    float3 baseColor0 = BaseColor0;
    float3 baseColor1 = BaseColor1;
    float3 baseColor2 = BaseColor2;
    float3 baseColor3 = BaseColor3;

    float3 pos = (float3) input.pos;

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
    float metalness0 = Metalness0;
    float metalness1 = Metalness1;
    float metalness2 = Metalness2;
    float metalness3 = Metalness3;

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

#if HasMetalnessTex0
    metalness0 = metalnessTexture0.Sample(metalnessTextureSampler0, float2(input.tex.xy)).r;
#endif
#if HasMetalnessTex1
    metalness1 = metalnessTexture1.Sample(metalnessTextureSampler1, float2(input.tex.xy)).r;
#endif
#if HasMetalnessTex2
    metalness2 = metalnessTexture2.Sample(metalnessTextureSampler2, float2(input.tex.xy)).r;
#endif
#if HasMetalnessTex3
    metalness3 = metalnessTexture3.Sample(metalnessTextureSampler3, float2(input.tex.xy)).r;
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

#if HasRoughnessMetalnessTex0
    float2 rm0 = roughnessMetalnessTexture0.Sample(roughnessMetalnessTextureSampler0, float2(input.tex.xy)).gb;
    roughness0 = rm0.x;
    metalness0 = rm0.y;
#endif
#if HasRoughnessMetalnessTex1
    float2 rm1 = roughnessMetalnessTexture1.Sample(roughnessMetalnessTextureSampler1, float2(input.tex.xy)).gb;
    roughness1 = rm1.x;
    metalness1 = rm1.y;
#endif
#if HasRoughnessMetalnessTex2
    float2 rm2 = roughnessMetalnessTexture2.Sample(roughnessMetalnessTextureSampler2, float2(input.tex.xy)).gb;
    roughness2 = rm2.x;
    metalness2 = rm2.y;
#endif
#if HasRoughnessMetalnessTex3
    float2 rm3 = roughnessMetalnessTexture3.Sample(roughnessMetalnessTextureSampler3, float2(input.tex.xy)).gb;
    roughness3 = rm3.x;
    metalness3 = rm3.y;
#endif

#if HasAmbientOcclusionRoughnessMetalnessTex0
    float3 orm0 = ambientOcclusionRoughnessMetalnessTexture0.Sample(ambientOcclusionRoughnessMetalnessSampler0, float2(input.tex.xy)).rgb;
    ao0 = orm0.r;
    roughness0 = orm0.g;
    metalness0 = orm0.b;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex1
    float3 orm1 = ambientOcclusionRoughnessMetalnessTexture1.Sample(ambientOcclusionRoughnessMetalnessSampler1, float2(input.tex.xy)).rgb;
    ao1 = orm1.r;
    roughness1 = orm1.g;
    metalness1 = orm1.b;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex2
    float3 orm2 = ambientOcclusionRoughnessMetalnessTexture2.Sample(ambientOcclusionRoughnessMetalnessSampler2, float2(input.tex.xy)).rgb;
    ao2 = orm2.r;
    roughness2 = orm2.g;
    metalness2 = orm2.b;
#endif
#if HasAmbientOcclusionRoughnessMetalnessTex3
    float3 orm3 = ambientOcclusionRoughnessMetalnessTexture3.Sample(ambientOcclusionRoughnessMetalnessSampler3, float2(input.tex.xy)).rgb;
    ao2 = orm2.r;
    roughness2 = orm2.g;
    metalness2 = orm2.b;
#endif

    float4 mask = maskTex.Sample(linearClampSampler, input.ctex).xyzw;

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
    metallic = lerp(metallic, metalness0, mask.x);
    metallic = lerp(metallic, metalness1, mask.y);
    metallic = lerp(metallic, metalness2, mask.z);
    metallic = lerp(metallic, metalness3, mask.w);

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

    if (opacity < 0.1f)
        discard;

    float3 N = normalize(normal);
    float3 V = normalize(GetCameraPos() - position);

    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor.rgb, metallic);

    float3 Lo = float3(0, 0, 0);

#if CLUSTERED_FORWARD
    uint tileIndex = GetClusterIndex(input.position.z / input.position.w, camNear, camFar, screenDim, float4(position, 1));

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
        [branch]
        switch (light.type)
        {
            case POINT_LIGHT:
                L += PointLightBRDF(light, position, F0, V, N, baseColor, roughness, metallic);
                break;
            case SPOT_LIGHT:
                L += SpotlightBRDF(light, position, F0, V, N, baseColor, roughness, metallic);
                break;
            case DIRECTIONAL_LIGHT:
                L += DirectionalLightBRDF(light, F0, V, N, baseColor, roughness, metallic);
                break;
        }

        float shadowFactor = 1;

        bool castsShadows = GetBit(light.castsShadows, 0);
        bool contactShadows = GetBit(light.castsShadows, 1);

        [branch]
        if (castsShadows)
        {
            ShadowData data = shadowData[light.shadowMapIndex];
            switch (light.type)
            {
                case POINT_LIGHT:
                    shadowFactor = ShadowFactorPointLight(shadowSampler, depthAtlas, light, data, position, N);
                    break;
                case SPOT_LIGHT:
                    shadowFactor = ShadowFactorSpotlight(shadowSampler, depthAtlas, light, data, position, N);
                    break;
                case DIRECTIONAL_LIGHT:
                    shadowFactor = ShadowFactorDirectionalLightCascaded(shadowSampler, depthCSM, light, data, position, N);
                    break;
            }
        }

        Lo += L * shadowFactor;
    }

    float3 ambient = baseColor * ambient_color.rgb;

    ambient = (ambient + BRDF_IBL(linearWrapSampler, globalDiffuse, globalSpecular, brdfLUT, F0, N, V, baseColor.xyz, roughness)) * ao;
    ambient += emissive;

#if HasBakedLightMap
    ambient += GetHBasisIrradiance(normal, input.H0, input.H1, input.H2, input.H3) / 3.14159f;
#endif

    Pixel output;
    output.Color = float4(ambient + Lo * ao, opacity);
    output.Normal = float4(PackNormal(N), opacity);

#if BAKE_FORWARD
	// For baking we render without backface culling, so that we still get occlusions inside meshes that
	// don't have full modeled interiors. If it is a backface triangle, we output pure black.
    output.Color = input.IsFrontFace ? output.Color : 0.0f;
#endif

    return output;
}