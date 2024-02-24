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
    output.ctex = input.pos.xz / TILESIZE;
    output.normal = mul(input.normal, (float3x3) world);
    output.tangent = mul(input.tangent, (float3x3) world);
    output.TessFactor = ComputeTessFactor(output.position);

    return output;
}
#else
PixelInput main(VertexInput input)
{
    PixelInput output;

    output.position = mul(float4(input.pos, 1), world);
    output.pos = output.position.xyz;
    output.position = mul(output.position, viewProj);
    output.tex = input.tex;
    output.ctex = input.pos.xz / TILESIZE;
    output.normal = mul(input.normal, (float3x3) world);
    output.tangent = mul(input.tangent, (float3x3) world);

    return output;
}
#endif