#include "../../world.hlsl"
#include "../../camera.hlsl"
struct VertexInputType
{
    float3 pos : POSITION;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
    float3 tex : POSITION;
};

PixelInputType main(VertexInputType input)
{
	PixelInputType output;

    output.position = mul(float4(input.pos, 1), world);
    output.position = mul(output.position, viewProj);

    output.tex = input.pos;
	return output;
}