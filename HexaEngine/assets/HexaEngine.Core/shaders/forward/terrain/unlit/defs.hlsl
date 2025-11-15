#ifndef INCLUDE_H_DEFS
#define INCLUDE_H_DEFS

#include "../common.hlsl"

struct VertexInput
{
    float3 position : POSITION;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float2 ctex : TEXCOORD1;
};

#endif