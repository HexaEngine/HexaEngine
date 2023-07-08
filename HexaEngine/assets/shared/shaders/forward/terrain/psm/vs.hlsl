#include "defs.hlsl"

cbuffer WorldBuffer
{
    float4x4 world;
};

cbuffer LightView : register(b1)
{
    matrix view;
};

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    output.position = mul(float4(input.pos, 1), world).xyzw;
    output.position = mul(output.position, view);
    output.depth = output.position.z / output.position.w;

    return output;
}