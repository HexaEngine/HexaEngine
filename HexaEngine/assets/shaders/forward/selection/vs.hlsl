#include "../../world.hlsl"
#include "../../camera.hlsl"

struct VertexInput
{
    uint vertex : SV_VertexID;
    uint instance : SV_InstanceID;
    float3 position : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    nointerpolation float4 color : COLOR;
};

PixelInput main(VertexInput input)
{
    PixelInput output;
    output.position = float4(input.position, 1);
    output.position = mul(output.position, world);
    output.position = mul(output.position, view);
    output.position = mul(output.position, proj);
    output.color = float4(input.vertex, 0, input.instance, 1);
	return output;
}