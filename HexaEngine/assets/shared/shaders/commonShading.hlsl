#ifndef INCLUDE_H_COMMON_SHADING
#define INCLUDE_H_COMMON_SHADING

#include "common.hlsl"
#include "camera.hlsl"
#include "light.hlsl"
#include "shadowCommon.hlsl"
#include "weather.hlsl"
#include "gbuffer.hlsl"
#include "cluster.hlsl"

SamplerState linearClampSampler : register(s0);
SamplerState linearWrapSampler : register(s1);
SamplerState pointClampSampler : register(s2);
SamplerComparisonState shadowSampler : register(s3);

Texture2D<float> ssao : register(t0);
Texture2D brdfLUT : register(t1);

StructuredBuffer<GlobalProbe> globalProbes : register(t2);
StructuredBuffer<Light> lights : register(t3);
StructuredBuffer<ShadowData> shadowData : register(t4);

StructuredBuffer<uint> lightIndexList : register(t5); //MAX_CLUSTER_LIGHTS * 16^3
StructuredBuffer<LightGrid> lightGrid : register(t6); //16^3

Texture2D<float> depthAtlas : register(t7);
Texture2DArray depthCSM : register(t8);

TextureCube globalDiffuse : register(t9);
TextureCube globalSpecular : register(t10);

float3 ComputeDirectLightning(
    float depth,
    float3 position,
    float3 viewDir,
    float3 surfaceNormal,
    float3 baseColor,
    float3 F0,
    float roughness,
    float metallic)
{
    float3 Lo = float3(0, 0, 0);

    uint tileIndex = GetClusterIndex(depth, camNear, camFar, screenDim, float4(position, 1));

    uint lightCount = lightGrid[tileIndex].lightCount;
    uint lightOffset = lightGrid[tileIndex].lightOffset;

    [loop]
    for (uint i = 0; i < lightCount; i++)
    {
        float3 L = 0;

        uint lightIndex = lightIndexList[lightOffset + i];
        Light light = lights[lightIndex];

        [branch]
        switch (light.type)
        {
            case POINT_LIGHT:
                L += PointLightBRDF(light, position, F0, viewDir, surfaceNormal, baseColor, roughness, metallic);
                break;
            case SPOT_LIGHT:
                L += SpotlightBRDF(light, position, F0, viewDir, surfaceNormal, baseColor, roughness, metallic);
                break;
            case DIRECTIONAL_LIGHT:
                L += DirectionalLightBRDF(light, F0, viewDir, surfaceNormal, baseColor, roughness, metallic);
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
                    shadowFactor = ShadowFactorPointLight(shadowSampler, depthAtlas, light, data, position, surfaceNormal);
                    break;
                case SPOT_LIGHT:
                    shadowFactor = ShadowFactorSpotlight(shadowSampler, depthAtlas, light, data, position, surfaceNormal);
                    break;
                case DIRECTIONAL_LIGHT:
                    shadowFactor = ShadowFactorDirectionalLightCascaded(shadowSampler, depthCSM, light, data, position, surfaceNormal);
                    break;
            }
        }

        Lo += L * shadowFactor;
    }

    return Lo;
}

float3 ComputeIndirectLightning(
    float2 screenUV,
    float3 viewDir,
    float3 surfaceNormal,
    float3 baseColor,
    float3 F0,
    float roughness,
    float metallic,
    float ao,
    float3 emissive)
{
    float3 ambient = baseColor * ambient_color.rgb;

    ao *= ssao.Sample(linearClampSampler, screenUV);

    ambient = (ambient + BRDF_IBL(linearClampSampler, globalDiffuse, globalSpecular, brdfLUT, F0, surfaceNormal, viewDir, baseColor.xyz, roughness)) * ao;
    ambient += emissive;

    return ambient;
}

#endif