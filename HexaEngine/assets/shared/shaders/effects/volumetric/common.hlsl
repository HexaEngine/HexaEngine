#ifndef VOLUMETRICS_COMMON_H_INCLUDED
#define VOLUMETRICS_COMMON_H_INCLUDED

#define HARD_SHADOWS_POINTLIGHTS 1

#include "../../common.hlsl"
#include "../../camera.hlsl"
#include "../../shadowCommon.hlsl"
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
Texture2D shadowAtlas : register(t1);
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

float PhaseFunction(float3 inDir, float3 outDir)
{
    float anisotropy = 0.0;
    float cosAngle = dot(inDir, outDir) / (length(inDir) * length(outDir));
    float nom = 1 - anisotropy * anisotropy;
    float denom = 4 * PI * pow(1 + anisotropy * anisotropy - 2 * anisotropy * cosAngle, 1.5);
    return nom / denom;
}

// Function to calculate Rayleigh scattering
float RayleighScattering(float3 V, float3 L)
{
    float cosTheta = dot(V, L);
    return rayleighCoefficient * (1.0 + cosTheta * cosTheta) / (4.0 * PI);
}

// Function to calculate Mie scattering
float MieScattering(float3 V, float3 L)
{
    float cosTheta = dot(V, L);
    float gSquared = mieG * mieG;
    float part1 = (1.0 - gSquared) * (1.0 + cosTheta * cosTheta);
    float part2 = pow(1.0 + gSquared - 2.0 * mieG * cosTheta, 1.5);
    return mieCoefficient * part1 / (4.0 * PI * part2);
}

#endif