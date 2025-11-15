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

    output.position = mul(float4(input.pos, 1), mat).xyz;
    output.tex = input.tex;
    output.ctex = input.pos.xz / TILESIZE;
    output.normal = mul(input.normal, (float3x3) mat);
    output.tangent = mul(input.tangent, (float3x3) mat);
    output.TessFactor = ComputeTessFactor(output.position);

    return output;
}
#else
PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.position = mul(float4(input.pos, 1), mat);
    output.pos = output.position.xyz;
    output.position = mul(output.position, viewProj);
    output.tex = input.tex;
    output.ctex = input.pos.xz / TILESIZE;
    output.normal = mul(input.normal, (float3x3) mat);
    output.tangent = mul(input.tangent, (float3x3) mat);

    return output;
}
#endif