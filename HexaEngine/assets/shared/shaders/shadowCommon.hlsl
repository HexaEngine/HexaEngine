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

float ShadowFactorPointLight(SamplerComparisonState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float3 N)
{
    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    // calculate bias (based on slope)
    //float bias = max(data.slopeBias * (1.0 - dot(N, L)), data.bias);
    const float bias = 0.005f;

    const float near = 0.001;
    const float far = light.range;

    // transform into lightspace
    float4 lightPos = mul(float4(position, 1), data.views[0]);

#if !HARD_SHADOWS_POINTLIGHTS

    float percentLit = 0.0f;

    const float dx = 1.0f / data.size;
    const float softness = data.softness;

    float2 offsets[9] =
    {
        float2(-dx, -dx), float2(0.0f, -dx), float2(dx, -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx, +dx), float2(0.0f, +dx), float2(dx, +dx)
    };

#endif

    if (lightPos.z >= 0.0f)
    {
        float4 posFront = paraboloid(lightPos, 1, near, far);
        float2 uvFront = float2(0.5, 0.5) + float2(0.5, 0.5) * (posFront.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        return shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvFront, data.regions[0]), posFront.z - bias);

#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            percentLit += shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvFront + offsets[i], data.regions[0]), posFront.z - bias).r;
        }

        return percentLit /= 9;

#endif
    }
    else
    {
        float4 posBack = paraboloid(lightPos, -1, near, far);
        float2 uvBack = float2(0.5, 0.5) + float2(0.5, 0.5) * (posBack.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        return shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvBack, data.regions[1]), posBack.z - bias);
#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            percentLit += shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvBack + offsets[i], data.regions[1]), posBack.z - bias).r;
        }

        return percentLit /= 9;

#endif
    }
}

float ShadowFactorPointLight(SamplerState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position, float3 N)
{
    float3 lightDirection = position - light.position.xyz;
    float3 L = normalize(lightDirection);

    //calculate bias (based on slope)
    //float bias = max(data.slopeBias * (1.0 - dot(N, L)), data.bias);
    const float bias = 0.005f;

    const float near = 0.001;
    const float far = light.range;

    // transform into lightspace
    float4 lightPos = mul(float4(position, 1), data.views[0]);

#if !HARD_SHADOWS_POINTLIGHTS

    float percentLit = 0.0f;

    const float dx = 1.0f / data.size;
    const float softness = data.softness;

    float2 offsets[9] =
    {
        float2(-dx, -dx), float2(0.0f, -dx), float2(dx, -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx, +dx), float2(0.0f, +dx), float2(dx, +dx)
    };

#endif

    if (lightPos.z >= 0.0f)
    {
        float4 posFront = paraboloid(lightPos, 1, near, far);
        float2 uvFront = float2(0.5, 0.5) + float2(0.5, 0.5) * (posFront.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvFront, data.regions[0]), 0);
        return 1.0f - ((posFront.z - bias > shadowDepth) ? 1.0f : 0.0f);

#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvFront + offsets[i], data.regions[0]), 0);
            percentLit += (posFront.z - bias > shadowDepth) ? 1.0f : 0.0f;
        }

        return percentLit /= 9;

#endif
    }
    else
    {
        float4 posBack = paraboloid(lightPos, -1, near, far);
        float2 uvBack = float2(0.5, 0.5) + float2(0.5, 0.5) * (posBack.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvBack, data.regions[1]), 0);
        return 1.0f - ((posBack.z - bias > shadowDepth) ? 1.0f : 0.0f);

#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvBack + offsets[i], data.regions[1]), 0);
            percentLit += (posBack.z - bias > shadowDepth) ? 1.0f : 0.0f;
        }

        return percentLit /= 9;

#endif
    }
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
    //float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);
    const float bias = 0.00001;

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
    //float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);
    const float bias = 0.00001;

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

float ShadowFactorPointLight(SamplerComparisonState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position)
{
    const float bias = 0.005f;

    const float near = 0.001;
    const float far = light.range;

    // transform into lightspace
    float4 lightPos = mul(float4(position, 1), data.views[0]);

#if !HARD_SHADOWS_POINTLIGHTS

    float percentLit = 0.0f;

    const float dx = 1.0f / data.size;
    const float softness = data.softness;

    float2 offsets[9] =
    {
        float2(-dx, -dx), float2(0.0f, -dx), float2(dx, -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx, +dx), float2(0.0f, +dx), float2(dx, +dx)
    };

#endif

    if (lightPos.z >= 0.0f)
    {
        float4 posFront = paraboloid(lightPos, 1, near, far);
        float2 uvFront = float2(0.5, 0.5) + float2(0.5, 0.5) * (posFront.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        return shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvFront, data.regions[0]), posFront.z - bias);

#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            percentLit += shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvFront + offsets[i], data.regions[0]), posFront.z - bias).r;
        }

        return percentLit /= 9;

#endif
    }
    else
    {
        float4 posBack = paraboloid(lightPos, -1, near, far);
        float2 uvBack = float2(0.5, 0.5) + float2(0.5, 0.5) * (posBack.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        return shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvBack, data.regions[1]), posBack.z - bias);
#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            percentLit += shadowAtlas.SampleCmpLevelZero(state, NormalizeShadowAtlasUV(uvBack + offsets[i], data.regions[1]), posBack.z - bias).r;
        }

        return percentLit /= 9;

#endif
    }
}

float ShadowFactorPointLight(SamplerState state, Texture2D shadowAtlas, Light light, ShadowData data, float3 position)
{
    const float bias = 0.005f;

    const float near = 0.001;
    const float far = light.range;

    // transform into lightspace
    float4 lightPos = mul(float4(position, 1), data.views[0]);

#if !HARD_SHADOWS_POINTLIGHTS

    float percentLit = 0.0f;

    const float dx = 1.0f / data.size;
    const float softness = data.softness;

    float2 offsets[9] =
    {
        float2(-dx, -dx), float2(0.0f, -dx), float2(dx, -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx, +dx), float2(0.0f, +dx), float2(dx, +dx)
    };

#endif

    if (lightPos.z >= 0.0f)
    {
        float4 posFront = paraboloid(lightPos, 1, near, far);
        float2 uvFront = float2(0.5, 0.5) + float2(0.5, 0.5) * (posFront.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvFront, data.regions[0]), 0);
        return 1.0f - ((posFront.z - bias > shadowDepth) ? 1.0f : 0.0f);

#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvFront + offsets[i], data.regions[0]), 0);
            percentLit += (posFront.z - bias > shadowDepth) ? 1.0f : 0.0f;
        }

        return percentLit /= 9;

#endif
    }
    else
    {
        float4 posBack = paraboloid(lightPos, -1, near, far);
        float2 uvBack = float2(0.5, 0.5) + float2(0.5, 0.5) * (posBack.xy * float2(1, -1));

#if HARD_SHADOWS_POINTLIGHTS

        float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvBack, data.regions[1]), 0);
        return 1.0f - ((posBack.z - bias > shadowDepth) ? 1.0f : 0.0f);

#else

        [unroll]
        for (int i = 0; i < 9; ++i)
        {
            offsets[i] = offsets[i] * float2(softness, softness);
            float shadowDepth = shadowAtlas.SampleLevel(state, NormalizeShadowAtlasUV(uvBack + offsets[i], data.regions[1]), 0);
            percentLit += (posBack.z - bias > shadowDepth) ? 1.0f : 0.0f;
        }

        return percentLit /= 9;

#endif
    }
}

float ShadowFactorSpotlight(SamplerComparisonState state, Texture2D shadowAtlas, ShadowData data, float3 position)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

    if (uvd.z > 1.0f)
        return 1.0;

    float depth = uvd.z;

    const float bias = 0.00001;

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

    const float bias = 0.00001;

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