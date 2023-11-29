#ifndef SHADOW_COMMON_H_INCLUDED
#define SHADOW_COMMON_H_INCLUDED

#include "light.hlsl"
#include "shadow.hlsl"
#include "camera.hlsl"

#ifndef HARD_SHADOWS_DIRECTIONAL
#define HARD_SHADOWS_DIRECTIONAL 0
#endif
#ifndef HARD_SHADOWS_DIRECTIONAL_CASCADED
#define HARD_SHADOWS_DIRECTIONAL_CASCADED 0
#endif
#ifndef HARD_SHADOWS_POINTLIGHTS
#define HARD_SHADOWS_POINTLIGHTS 0
#endif
#ifndef HARD_SHADOWS_SPOTLIGHTS
#define HARD_SHADOWS_SPOTLIGHTS 0
#endif

float ShadowFactorDirectionalLight(SamplerComparisonState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float N)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float3 L = normalize(position - light.position.xyz);

    float depth = uvd.z;

    // calculate bias (based on slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);

#if HARD_SHADOWS_DIRECTIONAL
    return shadowAtlas.SampleCmpLevelZero(state, float2(uvd.xy), depth - bias);
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
    for (int x = 0; x < 9; ++x)
    {
        offsets[x] = offsets[x] * float2(data.softness, data.softness);
        percentLit += shadowAtlas.SampleCmpLevelZero(state, float2(uvd.xy + offsets[x]), depth - bias);
    }
    return percentLit /= 9.0f;
#endif
}

float ShadowFactorDirectionalLight(SamplerState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float N)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float3 L = normalize(position - light.position.xyz);

    float depth = uvd.z;

    // calculate bias (based on slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);

#if HARD_SHADOWS_DIRECTIONAL
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
    return 1 - (percentLit /= 9.0f);
#endif
}

float ShadowFactorDirectionalLightCascaded(SamplerComparisonState state, Texture2DArray depthTex, Light light, ShadowData data, float3 position, float3 N)
{
    float cascadePlaneDistances[8] = (float[8]) data.cascades;

	// select cascade layer
    float4 fragPosViewSpace = mul(float4(position, 1.0), view);
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
    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;
    float3 L = normalize(position - light.position.xyz);

    // calculate bias (based on depth map resolution and slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);
    if (layer == data.cascadeCount)
    {
        bias *= 1 / (camFar * 0.5f);
    }
    else
    {
        bias *= 1 / (cascadePlaneDistance * 0.5f);
    }

#if HARD_SHADOWS_DIRECTIONAL_CASCADED
    return depthTex.SampleCmpLevelZero(state, float3(uvd.xy, layer), depth - bias);
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
    for (int x = 0; x < 9; ++x)
    {
        offsets[x] = offsets[x] * float2(data.softness, data.softness);
        percentLit += depthTex.SampleCmpLevelZero(state, float3(uvd.xy + offsets[x], layer), depth - bias);
    }
    return percentLit /= 9.0f;
#endif
}

static const float3 gridSamplingDisk[20] =
{
    float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
	float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
	float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
	float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
	float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

float ShadowFactorPointLight(SamplerComparisonState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float3 N, float viewDistance)
{
    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    float currentDepth = length(lightDirection) / light.range;

    if (currentDepth > 1.0f)
        return 1.0;

    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);

#if HARD_SHADOWS_POINTLIGHTS

    int face = GetPointLightFace(lightDirection);
    float2 uv = GetShadowAtlasUV(position, data.size, data.regions[face], data.views[face]);
    return shadowAtlas.SampleCmpLevelZero(state, uv.xy, currentDepth - bias);
#else

    float percentLit = 0.0f;
    float diskRadius = (1.0 + (viewDistance / camFar)) / 25.0;

    for (int i = 0; i < 20; ++i)
    {
        float3 newPosition = position + gridSamplingDisk[i] * diskRadius;
        lightDirection = newPosition - light.position.xyz;
        int face = GetPointLightFace(lightDirection);
        float2 uv = GetShadowAtlasUV(newPosition, data.size, data.regions[face], data.views[face]);
        percentLit += shadowAtlas.SampleCmpLevelZero(state, uv.xy, currentDepth - bias);
    }

    return percentLit /= 20;
#endif
}

float ShadowFactorPointLight(SamplerState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float3 N, float viewDistance)
{
    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    float depth = length(lightDirection) / light.range;

    if (depth > 1.0f)
        return 1.0;

    // calculate bias (based on slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);

#if HARD_SHADOWS_POINTLIGHTS

    int face = GetPointLightFace(lightDirection);
    float2 uv = GetShadowAtlasUV(position, data.size, data.regions[face], data.views[face]);
    float shadowDepth = shadowAtlas.SampleLevel(state, uv.xy, 0);
    return 1.0f - ((depth - bias > shadowDepth) ? 1.0f : 0.0f);
#else

    float percentLit = 0.0f;
    float diskRadius = (1.0 + (viewDistance / camFar)) / 25.0;

    for (int i = 0; i < 20; ++i)
    {
        float3 newPosition = position + gridSamplingDisk[i] * diskRadius;
        lightDirection = newPosition - light.position.xyz;
        int face = GetPointLightFace(lightDirection);
        float2 uv = GetShadowAtlasUV(newPosition, data.size, data.regions[face], data.views[face]);
        float shadowDepth = shadowAtlas.SampleLevel(state, uv, 0);
        percentLit += (depth - bias > shadowDepth) ? 1.0f : 0.0f;
    }

    return 1.0f - (percentLit /= 20);
#endif
}

float ShadowFactorSpotlight(SamplerComparisonState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float3 N)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    float depth = uvd.z;

    // calculate bias (based on slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);

#if HARD_SHADOWS_SPOTLIGHTS
    return shadowAtlas.SampleCmpLevelZero(state, uvd.xy, depth - bias);
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
        percentLit += shadowAtlas.SampleCmpLevelZero(state, uvd.xy + offsets[i], depth - bias);
    }
    return percentLit /= 9.0f;
#endif
}

float ShadowFactorSpotlight(SamplerState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float N)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    float depth = uvd.z;

    // calculate bias (based on slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);

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
    return 1 - (percentLit /= 9.0f);
#endif
}

float ShadowFactorDirectionalLight(SamplerComparisonState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;

    // const bias
    const float bias = 0.005;

#if HARD_SHADOWS_DIRECTIONAL
    return shadowAtlas.SampleCmpLevelZero(state, float2(uvd.xy), depth - bias);
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
    for (int x = 0; x < 9; ++x)
    {
        offsets[x] = offsets[x] * float2(data.softness, data.softness);
        percentLit += shadowAtlas.SampleCmpLevelZero(state, float2(uvd.xy + offsets[x]), depth - bias);
    }
    return percentLit /= 9.0f;
#endif
}

float ShadowFactorDirectionalLight(SamplerState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;

    // const bias
    const float bias = 0.005;

#if HARD_SHADOWS_DIRECTIONAL
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
    return 1 - (percentLit /= 9.0f);
#endif
}

float ShadowFactorDirectionalLightCascaded(SamplerComparisonState state, Texture2DArray depthTex, ShadowData data, float3 position)
{
    float cascadePlaneDistances[8] = (float[8]) data.cascades;

	// select cascade layer
    float4 fragPosViewSpace = mul(float4(position, 1.0), view);
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
    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;

    // const bias
    float bias = 0.005;

    if (layer == data.cascadeCount)
    {
        bias *= 1 / (camFar * 0.5f);
    }
    else
    {
        bias *= 1 / (cascadePlaneDistance * 0.5f);
    }

#if HARD_SHADOWS_DIRECTIONAL_CASCADED
    return depthTex.SampleCmpLevelZero(state, float3(uvd.xy, layer), depth - bias);
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
    for (int x = 0; x < 9; ++x)
    {
        offsets[x] = offsets[x] * float2(data.softness, data.softness);
        percentLit += depthTex.SampleCmpLevelZero(state, float3(uvd.xy + offsets[x], layer), depth - bias);
    }
    return percentLit /= 9.0f;
#endif
}

float ShadowFactorPointLight(SamplerComparisonState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float viewDistance)
{
    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    float currentDepth = length(lightDirection) / light.range;

    if (currentDepth > 1.0f)
        return 1.0;

    const float bias = 0.005;

#if HARD_SHADOWS_POINTLIGHTS

    int face = GetPointLightFace(lightDirection);
    float2 uv = GetShadowAtlasUV(position, data.size, data.regions[face], data.views[face]);
    return shadowAtlas.SampleCmpLevelZero(state, uv.xy, currentDepth - bias);
#else

    float percentLit = 0.0f;
    float diskRadius = (1.0 + (viewDistance / camFar)) / 25.0;

    for (int i = 0; i < 20; ++i)
    {
        float3 newPosition = position + gridSamplingDisk[i] * diskRadius;
        lightDirection = newPosition - light.position.xyz;
        int face = GetPointLightFace(lightDirection);
        float2 uv = GetShadowAtlasUV(newPosition, data.size, data.regions[face], data.views[face]);
        percentLit += shadowAtlas.SampleCmpLevelZero(state, uv.xy, currentDepth - bias);
    }

    return percentLit /= 20;
#endif
}

float ShadowFactorPointLight(SamplerState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float viewDistance)
{
    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    float depth = length(lightDirection) / light.range;

    if (depth > 1.0f)
        return 1.0;

    const float bias = 0.005;

#if HARD_SHADOWS_POINTLIGHTS

    int face = GetPointLightFace(lightDirection);
    float2 uv = GetShadowAtlasUV(position, data.size, data.regions[face], data.views[face]);
    float shadowDepth = shadowAtlas.SampleLevel(state, uv.xy, 0);
    return 1.0f - ((depth - bias > shadowDepth) ? 1.0f : 0.0f);
#else

    float percentLit = 0.0f;
    float diskRadius = (1.0 + (viewDistance / camFar)) / 25.0;

    for (int i = 0; i < 20; ++i)
    {
        float3 newPosition = position + gridSamplingDisk[i] * diskRadius;
        lightDirection = newPosition - light.position.xyz;
        int face = GetPointLightFace(lightDirection);
        float2 uv = GetShadowAtlasUV(newPosition, data.size, data.regions[face], data.views[face]);
        float shadowDepth = shadowAtlas.SampleLevel(state, uv, 0);
        percentLit += (depth - bias > shadowDepth) ? 1.0f : 0.0f;
    }

    return 1.0f - (percentLit /= 20);
#endif
}

float ShadowFactorSpotlight(SamplerComparisonState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;

    const float bias = 0.005;

#if HARD_SHADOWS_SPOTLIGHTS
    return shadowAtlas.SampleCmpLevelZero(state, uvd.xy, depth - bias);
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
        percentLit += shadowAtlas.SampleCmpLevelZero(state, uvd.xy + offsets[i], depth - bias);
    }
    return percentLit /= 9.0f;
#endif
}

float ShadowFactorSpotlight(SamplerState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;

    const float bias = 0.005;

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
    return 1 - (percentLit /= 9.0f);
#endif
}

#endif