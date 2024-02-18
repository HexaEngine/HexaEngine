#include "defs.hlsl"

#ifndef TILESIZE
#define TILESIZE float2(32, 32)
#endif

cbuffer WorldBuffer
{
    float4x4 world;
};
#if TESSELLATION

HullInput main(VertexInput input)
{
    HullInput output;

    output.position = mul(float4(input.pos, 1), world).xyz;
    output.tex = input.tex;
    output.TessFactor = ComputeTessFactor(output.position);

    return output;
}
#else
PixelInput main(VertexInput input)
{
    PixelInput output;

    output.position = mul(float4(input.pos, 1), world);
    output.position = mul(output.position, viewProj);
    output.ctex = input.pos.xz / TILESIZE;

    return output;
}
#endif