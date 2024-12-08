#include "defs.hlsl"

#ifndef TILESIZE
#define TILESIZE float2(32, 32)
#endif

cbuffer offsetBuffer
{
    uint offset;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

#if TESSELLATION

HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    HullInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.position = mul(float4(input.pos, 1), world).xyz;
    output.tex = input.tex;
    output.TessFactor = ComputeTessFactor(output.position);

    return output;
}
#else
PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.position = mul(float4(input.pos, 1), mat);
    output.position = mul(output.position, viewProj);
    output.ctex = input.pos.xz / TILESIZE;

    return output;
}
#endif