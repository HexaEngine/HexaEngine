#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer worldBuffer : register(b2)
{
    float4x4 world;
}

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
{
    PixelInput output;
    output.position = float4(input.pos, 1);
    output.position = mul(output.position, world);
    output.position = mul(output.position, viewProj);
    output.vertexId = vertexId;

    return output;
}