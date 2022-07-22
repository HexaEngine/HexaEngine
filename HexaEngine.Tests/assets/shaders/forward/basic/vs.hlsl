#include "../../world.hlsl"
#include "../../camera.hlsl"
struct VertexInputType
{
	float3 position : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 normal : NORMAL;
};

PixelInputType main(VertexInputType input)
{
	PixelInputType output;

    output.position = mul(float4(input.position, 1), world);
	output.position = mul(output.position, view);
    output.pos = output.position.xyz;
	output.position = mul(output.position, proj);
    output.normal = input.normal;

	return output;
}