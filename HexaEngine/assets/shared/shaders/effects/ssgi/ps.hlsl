#include "../../camera.hlsl"

#ifndef SSGI_QUALITY
#define SSGI_QUALITY 3
#endif

#if SSGI_QUALITY == 0
#define RAY_STEPS 8
#define RAY_COUNT 4
#elif SSGI_QUALITY == 1
#define RAY_STEPS 8
#define RAY_COUNT 8
#elif SSGI_QUALITY == 2
#define RAY_STEPS 8
#define RAY_COUNT 16
#elif SSGI_QUALITY == 3
#define RAY_STEPS 12
#define RAY_COUNT 32
#endif

Texture2D inputTx : register(t0);
Texture2D<float> depthTx : register(t1);

SamplerState linear_wrap_sampler : register(s0);

#define PI 3.14159265359

struct VertexOut
{
    float4 PosH : SV_POSITION;
    float2 Tex : TEXCOORD;
};

cbuffer SSGIBuffer : register(b0)
{
    float intensity;
    float distance;
};

float GetMipLevel(float2 offset)
{
    float v = clamp(pow(dot(offset, offset), 0.1), 0.0, 1.0);
    float lod = 10.0 * v;
    return lod;
}

float Rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float CalculateShadowRayCast(float3 startPosition, float3 endPosition, float2 startUV)
{
    float rayDistance = length(endPosition - startPosition);
    float3 rayDirection = normalize(endPosition - startPosition);

    float distancePerStep = rayDistance / RAY_STEPS;
    for (int i = 1; i < RAY_STEPS; i++)
    {
        float currentDistance = i * distancePerStep;
        float3 currentPosition = startPosition + rayDirection * currentDistance;

        float4 projectedPosition = mul(float4(currentPosition, 1.0f), proj);
        projectedPosition.xyz /= projectedPosition.w;
        float2 currentUV = projectedPosition.xy;

        float lod = GetMipLevel(currentUV - startUV);
        float projectedDepth = 1.0 / projectedPosition.z;

        float currentDepth = 1.0 / depthTx.SampleLevel(linear_wrap_sampler, currentUV, lod).r;
        if (projectedDepth > currentDepth + 0.1)
        {
            return float(i - 1) / RAY_STEPS;
        }
    }

    return 1.0;
}

float4 main(VertexOut pin) : SV_TARGET
{
    float depth = depthTx.SampleLevel(linear_wrap_sampler, pin.Tex, 0.0f);
    float3 fragment_position = GetPositionVS(pin.Tex, depth);

    float3 view_direction = normalize(0.0f - fragment_position);
    uint3 dimensions;
    inputTx.GetDimensions(0, dimensions.x, dimensions.y, dimensions.z);

    float2 inverse_size = 1.0f / float2(dimensions.xy);

    float3 accum = 0.0f;

    float r = Rand(pin.Tex);
    for (int i = 0; i < RAY_COUNT; i++)
    {
        float sampleDistance = exp(i - RAY_COUNT);
        float phi = ((i + r * RAY_COUNT) * 2.0 * PI) / RAY_COUNT;
        float2 uv = sampleDistance * float2(cos(phi), sin(phi));

        float lod = GetMipLevel(uv);
        float3 lightColor = inputTx.SampleLevel(linear_wrap_sampler, pin.Tex + uv, lod).rgb;
        float sampleDepth = depthTx.SampleLevel(linear_wrap_sampler, pin.Tex + uv, lod).r;
        float3 lightPosition = GetPositionVS(pin.Tex + uv, sampleDepth);
        float3 lightDirection = lightPosition - fragment_position.xyz;

        float currentDistance = length(lightDirection);
        float distanceAttenuation = clamp(1.0f - pow(currentDistance / distance, 4.0f), 0.0, 1.0);
        distanceAttenuation = isinf(currentDistance) ? 0.0 : distanceAttenuation;

        float shadowFactor = CalculateShadowRayCast(fragment_position.xyz, lightPosition, pin.Tex);

        accum += lightColor * shadowFactor * distanceAttenuation;
    }

    return float4(accum * intensity, 1.0);
}