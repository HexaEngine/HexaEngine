#include "../../world.hlsl"
#include "../../camera.hlsl"

struct VertexInputType
{
	float3 pos : POSITION;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float3 pos : POSITION;
	float3 tex : TEXCOORD;
};

PixelInputType main(VertexInputType input)
{
	PixelInputType output;

	output.position = mul(float4(input.pos, 1), world);
	output.pos = output.position.xyz;
	output.position = mul(output.position, viewProj);
	output.tex = normalize(input.pos.xyz);
	return output;
}