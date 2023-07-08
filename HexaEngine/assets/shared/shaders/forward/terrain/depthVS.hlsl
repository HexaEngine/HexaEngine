#include "../../camera.hlsl"

struct VertexInput
{
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float2 ctex : TEXCOORD1;
};

#ifndef TILESIZE
#define TILESIZE float2(32, 32)
#endif

cbuffer WorldBuffer
{
    float4x4 world;
};

Texture2D<float> Heightmap;

PixelInput main(VertexInput input)
{
    PixelInput output;

    output.position = mul(float4(input.pos, 1), world);
    output.position = mul(output.position, viewProj);
    output.ctex = input.pos.xz / TILESIZE;

    return output;
}