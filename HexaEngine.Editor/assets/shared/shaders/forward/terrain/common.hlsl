#ifndef INCLUDE_H_COMMON
#define INCLUDE_H_COMMON

#include "../../camera.hlsl"
#include "../../tessellation.hlsl"

inline float4 Mask(float4 current, float4 value, float mask)
{
    return current + value * mask;
}

inline float3 Mask(float3 current, float3 value, float mask)
{
    return current + value * mask;
}

inline float2 Mask(float2 current, float2 value, float mask)
{
    return current + value * mask;
}

inline float Mask(float current, float value, float mask)
{
    return current + value * mask;
}

#endif