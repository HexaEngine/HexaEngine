#include "defs.hlsl"

cbuffer cb
{
    uint offset;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

GeometryInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    GeometryInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.pos = mul(float4(input.pos, 1), mat).xyz;

    return output;
}