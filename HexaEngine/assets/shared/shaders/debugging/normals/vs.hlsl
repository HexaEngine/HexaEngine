#include "../../camera.hlsl"

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

struct GeometryInput
{
    float4 position : POSITION;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

GeometryInput main(VertexInputType input)
{
    GeometryInput output;

    output.position = mul(float4(input.position, 1), world);
    output.normal = normalize(mul(input.normal, (float3x3)world));
    output.tangent = normalize(mul(input.tangent, (float3x3)world));

	return output;
}