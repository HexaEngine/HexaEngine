#include "../../world.hlsl"
#include "../../camera.hlsl"
struct VertexInputType
{
    float3 pos : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 bitangent : BINORMAL;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float3 tex : TEXCOORD0;
};

PixelInputType main(VertexInputType input)
{
	PixelInputType output;

    output.position = mul(float4(input.pos, 1), world);
	output.position = mul(output.position, view);
	output.position = mul(output.position, proj);

    output.tex = (float3)normalize(input.pos);
	return output;
}