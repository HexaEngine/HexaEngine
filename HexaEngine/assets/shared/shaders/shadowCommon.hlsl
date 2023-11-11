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

#if HARD_SHADOWS_POINTLIGHTS
    return CalcShadowFactor_Basic(state, shadowAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, depthAtlas, uvd, data.size, data.softness);
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

#endif