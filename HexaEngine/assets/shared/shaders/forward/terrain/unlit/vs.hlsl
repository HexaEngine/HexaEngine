#include "defs.hlsl"

#ifndef TILESIZE
#define TILESIZE float2(32, 32)
#endif

cbuffer WorldBuffer
{
    float4x4 world;
};

PixelInput main(VertexInput input)
{
    PixelInput output;

    output.position = mul(float4(input.position, 1), world);
    output.ctex = input.position.xz / TILESIZE;
    output.position = mul(output.position, viewProj);

    return output;
}