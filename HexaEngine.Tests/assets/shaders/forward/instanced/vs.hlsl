#include "../../world.hlsl"
#include "../../camera.hlsl"
struct VertexInputType
{
	float3 position : POSITION;
	float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
    float4 instance : INSTANCED_MATS0;
    float4 instance1 : INSTANCED_MATS1;
    float4 instance2 : INSTANCED_MATS2;
    float4 instance3 : INSTANCED_MATS3;
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
    float4x4 mat = float4x4(input.instance, input.instance1, input.instance2, input.instance3);
    output.position = mul(float4(input.position, 1), mat);
	output.position = mul(output.position, view);
    output.pos = output.position.xyz;
	output.position = mul(output.position, proj);
    output.normal = input.normal;

	return output;
}