#include "defs.hlsl"

cbuffer WorldBuffer
{
    float4x4 world;
};

cbuffer LightBuffer : register(b1)
{
    matrix view;
    float3 position;
    float far;
};

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    output.position = mul(float4(input.pos, 1), world);
    output.depth = length(output.position.xyz - position) / far;
    output.position = mul(output.position, view);

    return output;
}