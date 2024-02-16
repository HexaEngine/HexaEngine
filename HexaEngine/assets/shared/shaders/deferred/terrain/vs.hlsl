#include "defs.hlsl"
#include "../../camera.hlsl"

#ifndef TILESIZE
#define TILESIZE float2(0, 0)
#endif

cbuffer WorldBuffer
{
    float4x4 world;
};

Texture2D<float> Heightmap;

PixelInput main(VertexInput input)
{
    PixelInput output;

    output.pos = mul(float4(input.pos, 1), world).xyzw;
    output.pos = mul(output.pos, viewProj);
    output.tex = input.tex;
    output.ctex = input.pos.xz / TILESIZE;
    output.normal = mul(input.normal, (float3x3) world);
    output.tangent = mul(input.tangent, (float3x3) world);

    return output;
}