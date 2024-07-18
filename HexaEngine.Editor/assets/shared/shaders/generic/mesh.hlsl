#include "../camera.hlsl"

cbuffer WorldBuffer : register(b0)
{
    float4x4 world;
};

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
    float4 pos : POSITION;
	float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

PixelInputType main(VertexInputType input)
{
	PixelInputType output;

    output.position = mul(float4(input.position, 1), world);
    output.pos = output.position;
    output.position = mul(output.position, viewProj);
	
    output.normal = normalize(mul(input.normal, (float3x3)world));
    output.tangent = normalize(mul(input.tangent, (float3x3)world));

    output.tex = input.tex;
	return output;
}