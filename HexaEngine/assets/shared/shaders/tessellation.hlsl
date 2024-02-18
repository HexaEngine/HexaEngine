#ifndef TESSELLATION_H_INCLUDED
#define TESSELLATION_H_INCLUDED

#ifndef MIN_DISTANCE
#define MIN_DISTANCE 2.0
#endif
#ifndef MAX_DISTANCE
#define MAX_DISTANCE 10.0
#endif
#ifndef MIN_TESS_FACTOR
#define MIN_TESS_FACTOR 1.0
#endif
#ifndef MAX_TESS_FACTOR
#define MAX_TESS_FACTOR 8.0
#endif

#include "camera.hlsl"

float3 ComputeDisplacement(Texture2D tex, SamplerState samplerState, float strength, in float3 position, in float3 uv, in float3 normal)
{
    float displacementValue = tex.SampleLevel(samplerState, uv.xy, 0).r * strength;
    position -= displacementValue * normal;
    return position;
}

float ComputeTessFactor(float3 position, float minTessFactor, float maxTessFactor, float minDistance, float maxDistance)
{
    float camDistance = distance(camPos, position);

    float tessellationFactor = lerp(maxTessFactor, minTessFactor, saturate((camDistance - minDistance) / (maxDistance - minDistance)));

    return tessellationFactor;
}

float ComputeTessFactor(float3 position)
{
    return ComputeTessFactor(position, MIN_TESS_FACTOR, MAX_TESS_FACTOR, MIN_DISTANCE, MAX_DISTANCE);
}

#endif