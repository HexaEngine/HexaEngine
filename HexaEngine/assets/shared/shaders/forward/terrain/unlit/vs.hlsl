#include "defs.hlsl"

#ifndef TILESIZE
#define TILESIZE float2(32, 32)
#endif

cbuffer cb
{
    uint offset;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.position = mul(float4(input.position, 1), mat);
    output.ctex = input.position.xz / TILESIZE;
    output.position = mul(output.position, viewProj);

    return output;
}