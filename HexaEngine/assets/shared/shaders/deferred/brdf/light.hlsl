#include "../../gbuffer.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"
#include "../../common.hlsl"
#include "../../shadow.hlsl"

#ifndef CLUSTERED_DEFERRED
#define CLUSTERED_DEFERRED 0
#endif

#if CLUSTERED_DEFERRED
#include "../../cluster.hlsl"
#endif

Texture2D GBufferA : register(t0);
Texture2D GBufferB : register(t1);
Texture2D GBufferC : register(t2);
Texture2D GBufferD : register(t3);
Texture2D<float> Depth : register(t4);
Texture2D ssao : register(t5);
StructuredBuffer<Light> lights : register(t6);
StructuredBuffer<ShadowData> shadowData : register(t7);

#if !CLUSTERED_DEFERRED
Texture2D depthAtlas : register(t8);
Texture2DArray depthCSM : register(t9);
#endif

#if CLUSTERED_DEFERRED
StructuredBuffer<uint> lightIndexList : register(t8); //MAX_CLUSTER_LIGHTS * 16^3
StructuredBuffer<LightGrid> lightGrid : register(t9); //16^3
Texture2D depthAtlas : register(t10);
Texture2DArray depthCSM : register(t11);
#endif

#if !CLUSTERED_DEFERRED
cbuffer constants : register(b0)
{
    uint cLightCount;
};
#endif

SamplerState linearClampSampler : register(s0);
SamplerState linearWrapSampler : register(s1);
SamplerState pointClampSampler : register(s2);
SamplerComparisonState shadowSampler : register(s3);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

/*
float InterleavedGradientNoise(float2 position_screen)
{
    position_screen += g_frame * any(g_taa_jitter_offset); // temporal factor
    float3 magic = float3(0.06711056f, 0.00583715f, 52.9829189f);
    return frac(magic.z * frac(dot(position_screen, magic.xy)));
}
*/

//https://panoskarabelas.com/posts/screen_space_shadows/
static const uint SSS_MAX_STEPS = 16; // Max ray steps, affects quality and performance.
static const float SSS_RAY_MAX_DISTANCE = 0.05f; // Max shadow length, longer shadows are less accurate.
static const float SSS_THICKNESS = 0.5f; // Depth testing thickness.
static const float SSS_STEP_LENGTH = SSS_RAY_MAX_DISTANCE / (float) SSS_MAX_STEPS;

float SSCS(float3 position, float2 uv, float3 direction)
{
    // Compute ray position and direction (in view-space)
    float3 ray_pos = mul(float4(position, 1.0f), view).xyz;
    float3 ray_dir = mul(float4(-direction, 0.0f), view).xyz;

    // Compute ray step
    float3 ray_step = ray_dir * SSS_STEP_LENGTH;

    //float offset = InterleavedGradientNoise(screen_dim * uv) * 2.0f - 1.0f;
    //ray_pos += ray_step * offset;

    // Ray march towards the light
    float occlusion = 0.0;
    float2 ray_uv = 0.0f;
    for (uint i = 0; i < SSS_MAX_STEPS; i++)
    {
        // Step the ray
        ray_pos += ray_step;
        ray_uv = ProjectUV(ray_pos, proj);

        // Ensure the UV coordinates are inside the screen
        if (IsSaturated(ray_uv))
        {
            // Compute the difference between the ray's and the camera's depth
            float depth_z = SampleLinearDepth(Depth, linearClampSampler, ray_uv);
            float depth_delta = ray_pos.z - depth_z;

            // Check if the camera can't "see" the ray (ray depth must be larger than the camera depth, so positive depth_delta)
            if ((depth_delta > 0.0f) && (depth_delta < SSS_THICKNESS))
            {
                // Mark as occluded
                occlusion = 1.0f;

                // Fade out as we approach the edges of the screen
                occlusion *= ScreenFade(ray_uv);

                break;
            }
        }
    }

    // Convert to visibility
    return 1.0f - occlusion;
}

float ShadowFactorSpotlight(ShadowData data, float3 position, SamplerComparisonState state)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

#if HARD_SHADOWS_SPOTLIGHTS
    return CalcShadowFactor_Basic(state, depthAtlas, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, depthAtlas, uvd, data.size, data.softness);
#endif
}

// array of offset direction for sampling
static const float3 gridSamplingDisk[20] =
{
    float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
	float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
	float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
	float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
	float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

#define HARD_SHADOWS_POINTLIGHTS 1
float ShadowFactorPointLight(ShadowData data, Light light, float3 position, float viewDistance, SamplerComparisonState state)
{
    float3 lightDirection = position - light.position.xyz;

    float currentDepth = length(lightDirection) / light.range;

    if (currentDepth > 1.0f)
        return 1.0;

    const float bias = 0.005f;

#if HARD_SHADOWS_POINTLIGHTS

    int face = GetPointLightFace(lightDirection);
    float2 uv = GetShadowAtlasUV(position, data.size, data.regions[face], data.views[face]);
    return depthAtlas.SampleCmpLevelZero(state, uv.xy, currentDepth - bias);
#else
    float percentLit = 0.0f;
    float diskRadius = (1.0 + (viewDistance / camFar)) / 25.0;

    for (int i = 0; i < 20; ++i)
    {
        float3 newPosition = position + gridSamplingDisk[i] * diskRadius;
        lightDirection = newPosition - light.position.xyz;
        int face = GetPointLightFace(lightDirection);
        float2 uv = GetShadowAtlasUV(newPosition, data.size, data.regions[face], data.views[face]);
        percentLit += depthAtlas.SampleCmpLevelZero(state, uv.xy, currentDepth - bias);
    }

    return percentLit /= 20;
#endif
}

float ShadowFactorDirectionalLight(ShadowData data, float3 position, Texture2D depthTex, SamplerComparisonState state)
{
    float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

#if HARD_SHADOWS_DIRECTIONAL
    return CalcShadowFactor_Basic(state, depthTex, uvd);
#else
    return CalcShadowFactor_PCF3x3(state, depthTex, uvd, data.size, 1);
#endif
}

float ShadowFactorDirectionalLightCascaded(SamplerComparisonState state, Texture2DArray depthTex, Light light, ShadowData data, float camFar, float4x4 camView, float3 position, float3 N)
{
    float cascadePlaneDistances[8] = (float[8]) data.cascades;
    float farPlane = camFar;

	// select cascade layer
    float4 fragPosViewSpace = mul(float4(position, 1.0), camView);
    float depthValue = abs(fragPosViewSpace.z);
    float cascadePlaneDistance;
    uint layer = data.cascadeCount;
    for (uint iLayer = 0; iLayer < data.cascadeCount; ++iLayer)
    {
        if (depthValue < cascadePlaneDistances[iLayer])
        {
            cascadePlaneDistance = cascadePlaneDistances[iLayer];
            layer = iLayer;
            break;
        }
    }

    float3 uvd = GetShadowUVD(position, data.views[layer]);

    float currentDepth = uvd.z;

    if (currentDepth > 1.0)
    {
        return 0.0;
    }

    float3 L = normalize(position - light.position.xyz);

    // calculate bias (based on depth map resolution and slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);
    if (layer == data.cascadeCount)
    {
        bias *= 1 / (farPlane * 0.5f);
    }
    else
    {
        bias *= 1 / (cascadePlaneDistances[layer] * 0.5f);
    }

#ifdef HARD_SHADOWS_DIRECTIONAL_CASCADED
    return depthTex.SampleCmpLevelZero(state, float3(uvd.xy, layer), currentDepth - bias);
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
    for (uint i = 0; i < 9; ++i)
    {
        offsets[i] = offsets[i] * float2(data.softness, data.softness);
        percentLit += depthTex.SampleCmpLevelZero(state, float3(uvd.xy + offsets[i], layer), currentDepth - bias);
    }
    return (percentLit /= 9.0f);
#endif
}

float ShadowFactorDirectionalLightCascaded(SamplerState state, Texture2DArray depthTex, Light light, ShadowData data, float camFar, float4x4 camView, float3 position, float3 N)
{
    float cascadePlaneDistances[8] = (float[8]) data.cascades;
    float farPlane = camFar;

	// select cascade layer
    float4 fragPosViewSpace = mul(float4(position, 1.0), camView);
    float depthValue = abs(fragPosViewSpace.z);
    float cascadePlaneDistance;
    uint layer = data.cascadeCount;
    for (uint iLayer = 0; iLayer < data.cascadeCount; ++iLayer)
    {
        if (depthValue < cascadePlaneDistances[iLayer])
        {
            cascadePlaneDistance = cascadePlaneDistances[iLayer];
            layer = iLayer;
            break;
        }
    }

    float3 uvd = GetShadowUVD(position, data.views[layer]);

    float depth = uvd.z;

    if (depth > 1.0)
    {
        return 0.0;
    }

    float3 L = normalize(position - light.position.xyz);

    // calculate bias (based on depth map resolution and slope)
    float bias = max(0.05 * (1.0 - dot(N, L)), 0.005);
    if (layer == data.cascadeCount)
    {
        bias *= 1 / (farPlane * 0.5f);
    }
    else
    {
        bias *= 1 / (cascadePlaneDistances[layer] * 0.5f);
    }

#ifdef HARD_SHADOWS_DIRECTIONAL_CASCADED
    float shadowDepth = depthTex.SampleLevel(state, float3(uvd.xy, layer), 0);
    float shadowFactor = (depth - bias > shadowDepth) ? 1.0f : 0.0f;
    return 1 - shadowFactor;
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
    for (uint i = 0; i < 9; ++i)
    {
        offsets[i] = offsets[i] * float2(data.softness, data.softness);
        float shadowDepth = depthTex.SampleLevel(state, float3(uvd.xy + offsets[i], layer), 0);
        percentLit += (depth - bias > shadowDepth) ? 1.0f : 0.0f;
    }
    return 1 - (percentLit /= 9.0f);
#endif
}

float ContactShadowsSpotlight(float3 position, float2 uv, Light light)
{
    return SSCS(position, uv, light.direction.xyz);
}

float ContactShadowsPointLight(float3 position, float2 uv, Light light)
{
    float3 LN = light.position.xyz - position;
    float distance = length(LN);
    float3 L = normalize(LN);
    return SSCS(position, uv, L);
}

#if CLUSTERED_DEFERRED
float4 ComputeLightingPBR(VSOut input, float3 position, const uint tileIndex, GeometryAttributes attrs)
#else
float4 ComputeLightingPBR(VSOut input, float3 position, GeometryAttributes attrs)
#endif
{
#if CLUSTERED_DEFERRED
    uint lightCount = lightGrid[tileIndex].lightCount;
    uint lightOffset = lightGrid[tileIndex].lightOffset;
#else
    uint lightCount = cLightCount;
#endif

    float3 baseColor = attrs.baseColor;
    float roughness = attrs.roughness;
    float metallic = attrs.metallic;

    float3 N = normalize(attrs.normal);
    float3 VN = GetCameraPos() - position;
    float3 V = normalize(VN);

    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metallic);

    float3 Lo = float3(0, 0, 0);

    [loop]
    for (uint i = 0; i < lightCount; i++)
    {
        float3 L = 0;
#if CLUSTERED_DEFERRED
        uint lightIndex = lightIndexList[lightOffset + i];
        Light light = lights[lightIndex];
#else
        Light light = lights[i];
#endif

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

        if (castsShadows)
        {
            ShadowData data = shadowData[light.shadowMapIndex];
            switch (light.type)
            {
                case POINT_LIGHT:
                    shadowFactor = ShadowFactorPointLight(data, light, position, length(VN), shadowSampler);
                    break;
                case SPOT_LIGHT:
                    shadowFactor = ShadowFactorSpotlight(data, position, shadowSampler);
                    break;
                case DIRECTIONAL_LIGHT:
                    shadowFactor = ShadowFactorDirectionalLightCascaded(shadowSampler, depthCSM, light, data, camFar, view, position, N);
                    break;
            }
        }

        if (contactShadows)
        {
            switch (light.type)
            {
                case POINT_LIGHT:
                    shadowFactor *= ContactShadowsPointLight(position, input.Tex, light);
                    break;
                case SPOT_LIGHT:
                    shadowFactor *= ContactShadowsSpotlight(position, input.Tex, light);
                    break;
                case DIRECTIONAL_LIGHT:
                    shadowFactor *= ContactShadowsSpotlight(position, input.Tex, light);
                    break;
            }
        }

        Lo += L * shadowFactor;
    }

    return float4(Lo, 1);
}

float4 main(VSOut pixel) : SV_TARGET
{
    float depth = Depth.Sample(linearWrapSampler, pixel.Tex);
    float3 position = GetPositionWS(pixel.Tex, depth);
    GeometryAttributes attrs;
    ExtractGeometryData(pixel.Tex, GBufferA, GBufferB, GBufferC, GBufferD, linearWrapSampler, attrs);

#if CLUSTERED_DEFERRED
    uint tileIndex = GetClusterIndex(GetLinearDepth(depth), camNear, camFar, screenDim, float4(position, 1));

    return ComputeLightingPBR(pixel, position, tileIndex, attrs);
#else
    return ComputeLightingPBR(pixel, position, attrs);
#endif
}