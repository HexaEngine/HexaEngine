#ifndef INCLUDE_H_DEFS
#define INCLUDE_H_DEFS

#include "../common.hlsl"

struct VertexInput
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct HullInput
{
    float3 position : POSITION;
    float2 tex : TEXCOORD;
    float TessFactor : TESS;
};

struct DomainInput
{
    float3 position : POSITION;
    float2 tex : TEXCOORD;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float2 ctex : TEXCOORD1;
};

struct PatchTess
{
    float EdgeTess[3] : SV_TessFactor;
    float InsideTess : SV_InsideTessFactor;
};

#endif