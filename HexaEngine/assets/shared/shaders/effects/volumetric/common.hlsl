#ifndef VOLUMETRICS_COMMON_H_INCLUDED
#define VOLUMETRICS_COMMON_H_INCLUDED

#define HARD_SHADOWS_DIRECTIONAL 1
#define HARD_SHADOWS_DIRECTIONAL_CASCADED 1
#define HARD_SHADOWS_POINTLIGHTS 1
#define HARD_SHADOWS_SPOTLIGHTS 1

#include "../../common.hlsl"
#include "../../camera.hlsl"
#include "../../commonShadows.hlsl"
#include "../../light.hlsl"
#include "../../dither.hlsl"
#include "../../math.hlsl"

#ifndef VOLUMETRIC_LIGHT_QUALITY
#define VOLUMETRIC_LIGHT_QUALITY 2
#endif

#if VOLUMETRIC_LIGHT_QUALITY == 0
#define SAMPLE_COUNT 4
#endif
#if VOLUMETRIC_LIGHT_QUALITY == 1
#define SAMPLE_COUNT 8
#endif
#if VOLUMETRIC_LIGHT_QUALITY == 2
#define SAMPLE_COUNT 16
#endif
#if VOLUMETRIC_LIGHT_QUALITY == 3
#define SAMPLE_COUNT 32
#endif

struct VolumetricLight
{
    Light light;
    float volumetricStrength;
};

SamplerState linear_clamp_sampler : register(s0);
SamplerComparisonState shadow_sampler : register(s1);

Texture2D<float> depthTx : register(t0);
Texture2D<float> shadowAtlas : register(t1);
Texture2DArray cascadeShadowMaps : register(t2);

StructuredBuffer<VolumetricLight> lights : register(t3);
StructuredBuffer<ShadowData> shadowData : register(t4);

cbuffer constants : register(b0)
{
    uint lightCount;
    uint3 padd0;
    float density;
    float rayleighCoefficient;
    float mieCoefficient;
    float mieG;
};

struct VertexOut
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

float HenyeyGreenstein(float3 V, float3 L)
{
    float cosTheta = dot(V, L);
    return (1.0 - mieG * mieG) / (4.0 * PI * pow(abs(1.0 + mieG * mieG - 2.0 * mieG * cosTheta), 1.5));
}

#endif