#ifndef SHADOW_COMMON_H_INCLUDED
#define SHADOW_COMMON_H_INCLUDED

#include "light.hlsl"
#include "shadow.hlsl"

#ifndef HARD_SHADOWS_DIRECTIONAL
#define HARD_SHADOWS_DIRECTIONAL 0
#endif
#ifndef HARD_SHADOWS_DIRECTIONAL_CASCADED
#define HARD_SHADOWS_DIRECTIONAL_CASCADED 0
#endif
#ifndef HARD_SHADOWS_POINTLIGHTS
#define HARD_SHADOWS_POINTLIGHTS 1
#endif
#ifndef HARD_SHADOWS_SPOTLIGHTS
#define HARD_SHADOWS_SPOTLIGHTS 0
#endif

float ShadowFactorDirectionalLight(SamplerComparisonState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

#if HARD_SHADOWS_DIRECTIONAL
    return CalcShadowFactor_Basic(state, shadowAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, shadowAtlas, uvd, data.size, 1);
#endif
}

float ShadowFactorDirectionalLightCascaded(SamplerComparisonState state, Texture2DArray depthTex, ShadowData data, float camFar, float4x4 camView, float3 position)
{
    float cascadePlaneDistances[8] = (float[8]) data.cascades;
    float farPlane = camFar;

	// select cascade layer
    float4 fragPosViewSpace = mul(float4(position, 1.0), camView);
    float depthValue = abs(fragPosViewSpace.z);
    float cascadePlaneDistance;
    uint layer = data.cascadeCount;
    for (uint i = 0; i < (uint) data.cascadeCount; ++i)
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

float ShadowFactorPointLight(SamplerComparisonState state, Texture2D shadowAtlas, ShadowData data, Light light, float3 position)
{
    float3 lightDirection = position - light.position.xyz;

    int face = GetPointLightFace(lightDirection);
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[face], data.views[face]);
    uvd.z = length(lightDirection) / light.range;

#if HARD_SHADOWS_POINTLIGHTS
    return CalcShadowFactor_Basic(state, shadowAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, depthAtlas, uvd, data.size, data.softness);
#endif
}

float ShadowFactorPointLight(SamplerState state, Texture2D shadowAtlas, ShadowData data, Light light, float3 position, float viewDistance)
{
    float3 lightDirection = position - light.position.xyz;

    float currentDepth = length(lightDirection) / light.range;

    if (currentDepth > 1.0f)
        return 1.0;

    const float bias = 0.005f;

#if HARD_SHADOWS_POINTLIGHTS

    int face = GetPointLightFace(lightDirection);
    float2 uv = GetShadowAtlasUV(position, data.size, data.regions[face], data.views[face]);

    float shadowDepth = shadowAtlas.SampleLevel(state, uv.xy, 0);

    float shadowFactor = (currentDepth - bias > shadowDepth) ? 1.0f : 0.0f;

    return (1.0f - shadowFactor);
#else

    float percentLit = 0.0f;
    float diskRadius = (1.0 + (viewDistance / camFar)) / 25.0;

    for (int i = 0; i < 20; ++i)
    {
        float3 newPosition = position + gridSamplingDisk[i] * diskRadius;
        lightDirection = newPosition - light.position.xyz;
        int face = GetPointLightFace(lightDirection);
        float2 uv = GetShadowAtlasUV(newPosition, data.size, data.regions[face], data.views[face]);
        float shadowDepth = shadowAtlas.SampleLevel(state, uv.xy, 0);
        percentLit += (currentDepth - bias > shadowDepth) ? 1.0f : 0.0f;
    }

    return 1 - (percentLit / 20);
#endif
}

float ShadowFactorSpotlight(SamplerComparisonState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

#if HARD_SHADOWS_SPOTLIGHTS
    return CalcShadowFactor_Basic(state, shadowAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, shadowAtlas, uvd, data.size, data.softness);
#endif
}

float ShadowFactorSpotlight(SamplerState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;

    const float bias = 0.005f;

#if HARD_SHADOWS_SPOTLIGHTS
    float shadowDepth = shadowAtlas.SampleLevel(state, uvd.xy, 0);
    float shadowFactor = (depth - bias > shadowDepth) ? 1.0f : 0.0f;
    return (1.0f - shadowFactor);
#else
    const float dx = 1.0f / data.size;

    float percentLit = 0.0f;

    float2 offsets[9] =
    {
        float2(-dx, -dx), float2(0.0f, -dx), float2(dx, -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx, +dx), float2(0.0f, +dx), float2(dx, +dx)
    };

	[unroll]
    for (int i = 0; i < 9; ++i)
    {
        offsets[i] = offsets[i] * float2(data.softness, data.softness);
        float shadowDepth = shadowAtlas.SampleLevel(state, uvd.xy + offsets[i], 0);
        percentLit += (depth - bias > shadowDepth) ? 1.0f : 0.0f;
    }
    return percentLit /= 9.0f;
#endif
}

#endif